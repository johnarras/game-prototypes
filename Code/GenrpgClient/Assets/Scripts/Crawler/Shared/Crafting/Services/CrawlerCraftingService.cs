using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Crafting.Settings;
using Genrpg.Shared.Crawler.Currencies.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.Shared.Crafting.Services
{
    public interface ICrawlerCraftingService : IInjectable
    {
        bool ScrapItem(PartyData party, Item item, Vector3 startPos);
    }

    public class CrawlerCraftingService : ICrawlerCraftingService
    {
        private IClientGameState _gs = null;
        private IGameData _gameData = null;
        private IClientRandom _rand = null;
        private IDispatcher _dispatcher = null;
        private IDynamicUIService _dynamicUIService = null;

        public bool ScrapItem(PartyData party, Item item, Vector3 startPos)
        {
            Dictionary<long, long> currencyCounts = new Dictionary<long, long>();

            CrawlerCurrencySettings currencySettings = _gameData.Get<CrawlerCurrencySettings>(_gs.ch);

            IReadOnlyList<CrawlerCurrencyType> currencies = currencySettings.GetData();

            CrawlerCraftingSettings craftingSettings = _gameData.Get<CrawlerCraftingSettings>(_gs.ch);

            List<StatType> primaryStats = _gameData.Get<StatSettings>(_gs.ch).GetData().Where(x => StatConstants.IsPrimaryStat(x.IdKey)).ToList();

            foreach (ItemEffect eff in item.Effects)
            {
                if (eff.EntityTypeId == EntityTypes.Stat)
                {
                    CrawlerCurrencyType ctype = currencies.FirstOrDefault(x => x.CraftingStatTypeId == eff.EntityId);

                    if (ctype != null)
                    {
                        if (!currencyCounts.ContainsKey(ctype.IdKey))
                        {
                            currencyCounts[ctype.IdKey] = 0;
                        }
                        for (int i = 0; i < eff.Quantity; i++)
                        {
                            if (_rand.NextDouble() < craftingSettings.ScrapReagentChance)
                            {
                                currencyCounts[ctype.IdKey]++;
                            }
                        }
                    }
                }
            }

            if (currencyCounts.Keys.Count > 0 && currencyCounts.Values.Sum() < 1)
            {
                long randId = currencyCounts.Keys.ToList()[_rand.Next(currencyCounts.Keys.Count)];
                currencyCounts[randId]++;
            }

            foreach (long idkey in currencyCounts.Keys)
            {
                if (currencyCounts[idkey] > 0)
                {
                    party.Currencies.Add(idkey, currencyCounts[idkey]);
                    _dynamicUIService.ShowEntityDooberWithStartPosition(EntityTypes.CrawlerCurrency, idkey, currencyCounts[idkey], true, startPos);
                }
            }

            party.Inventory.Remove(item);

            return true;
        }
    }
}



using Assets.Scripts.DynamicUI.Services;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Crafting.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
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
        private IDynamicUIService _dynamicUIService = null;

        public bool ScrapItem(PartyData party, Item item, Vector3 startPos)
        {
            Dictionary<long, long> currencyCounts = new Dictionary<long, long>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch);

            IReadOnlyList<CoreCurrencyType> currencies = currencySettings.GetData();

            CrawlerCraftingSettings craftingSettings = _gameData.Get<CrawlerCraftingSettings>(_gs.ch);

            List<StatType> primaryStats = _gameData.Get<StatSettings>(_gs.ch).GetData().Where(x => StatConstants.IsPrimaryStat(x.IdKey)).ToList();

            foreach (Effect eff in item.Effects)
            {
                if (eff.EntityTypeId == EntityTypes.Stat)
                {
                    CoreCurrencyType ctype = currencies.FirstOrDefault(x => x.StatTypeId == eff.EntityId);

                    if (ctype != null)
                    {
                        if (!currencyCounts.ContainsKey(ctype.IdKey))
                        {
                            currencyCounts[ctype.IdKey] = 0;
                        }
                        for (int i = 0; i < eff.Quantity; i++)
                        {
                            if (_gs.Rand.NextDouble() < craftingSettings.ScrapReagentChance)
                            {
                                currencyCounts[ctype.IdKey]++;
                            }
                        }
                    }
                }
            }

            if (currencyCounts.Keys.Count > 0 && currencyCounts.Values.Sum() < 1)
            {
                long randId = currencyCounts.Keys.ToList()[_gs.Rand.Next(currencyCounts.Keys.Count)];
                currencyCounts[randId]++;
            }

            foreach (long idkey in currencyCounts.Keys)
            {
                if (currencyCounts[idkey] > 0)
                {
                    party.Currencies.Add(idkey, currencyCounts[idkey]);
                    _dynamicUIService.ShowEntityDooberWithStartPosition(EntityTypes.CoreCurrency, idkey, currencyCounts[idkey], true, startPos);
                }
            }

            party.Inventory.Remove(item);

            return true;
        }
    }
}



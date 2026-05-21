using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Loot.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public class CrawlerSpellLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long HelperKey => EntityTypes.CrawlerSpell;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            CrawlerLootType lootType = _gameData.Get<CrawlerLootSettings>(_gs.ch).Get(HelperKey);

            long effectLevel = (long)(1 + (args.Level * lootType.ScalingPerLevel));

            List<CrawlerSpell> okSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().Where(x => x.RoleScalingTier <= effectLevel).ToList();

            CrawlerSpell spell = RandUtils.GetRandomEnchant(okSpells, _rand.Rand);

            if (spell != null && lootType != null)
            {
                item.Effects.Add(new Effect()
                {
                    EntityTypeId = EntityTypes.CrawlerSpell,
                    EntityId = spell.IdKey,
                    Quantity = effectLevel,
                });
            }
        }
    }
}



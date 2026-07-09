using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Spells.Settings.Elements;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public class ElementLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long HelperKey => EntityTypes.Element;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            IReadOnlyList<ElementType> etypes = _gameData.Get<ElementTypeSettings>(_gs.ch).GetData();

            if (etypes.Count < 1)
            {
                return;
            }

            ElementType etype = etypes[_gs.Rand.Next(etypes.Count)];

            item.Effects.Add(new Effect()
            {
                EntityTypeId = EntityTypes.Element,
                EntityId = etype.IdKey,
                Quantity = 1,
            });
        }
    }
}



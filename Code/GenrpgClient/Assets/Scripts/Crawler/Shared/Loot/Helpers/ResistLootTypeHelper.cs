using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Spells.Settings.Elements;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public class ResistLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long Key => EntityTypes.Resist;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            IReadOnlyList<ElementType> etypes = _gameData.Get<ElementTypeSettings>(_gs.ch).GetData();

            if (etypes.Count < 1)
            {
                return;
            }

            ElementType etype = etypes[_rand.Next(etypes.Count)];

            item.Effects.Add(new ItemEffect()
            {
                EntityTypeId = EntityTypes.Resist,
                EntityId = etype.IdKey,
                Quantity = 1,
            });
        }
    }
}

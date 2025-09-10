using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public class MapMagicLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long Key => EntityTypes.MapMagic;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            MapMagicType magicType = RandomUtils.GetRandomEnchant(_gameData.Get<MapMagicSettings>(_gs.ch).GetData(), _rand);

            if (magicType != null)
            {
                item.Effects.Add(new ItemEffect()
                {
                    EntityTypeId = EntityTypes.MapMagic,
                    EntityId = magicType.IdKey,
                    Quantity = 1,
                });
            }
        }
    }
}

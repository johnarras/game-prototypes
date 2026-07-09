using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public class MapMagicLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long HelperKey => EntityTypes.MapMagic;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            MapMagicType magicType = RandUtils.GetRandomEnchant(_gameData.Get<MapMagicSettings>(_gs.ch).GetData(), _gs.Rand);

            if (magicType != null)
            {
                item.Effects.Add(new Effect()
                {
                    EntityTypeId = EntityTypes.MapMagic,
                    EntityId = magicType.IdKey,
                    Quantity = 1,
                });
            }
        }
    }
}



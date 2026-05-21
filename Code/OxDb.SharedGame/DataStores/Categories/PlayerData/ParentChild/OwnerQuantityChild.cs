using OxDb.SharedGame.Interfaces;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild
{
    public abstract class OwnerQuantityChild : OwnerPlayerData, IOwnerQuantityChild
    {

        [MessagePack.IgnoreMember]
        public abstract long IdKey { get; set; }

        [MessagePack.IgnoreMember]
        public abstract long Quantity { get; set; }
    }
}



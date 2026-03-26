using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    public abstract class OwnerQuantityChild : OwnerPlayerData, IOwnerQuantityChild
    {

        [MessagePack.IgnoreMember]
        public abstract long IdKey { get; set; }

        [MessagePack.IgnoreMember]
        public abstract long Quantity { get; set; }
    }
}



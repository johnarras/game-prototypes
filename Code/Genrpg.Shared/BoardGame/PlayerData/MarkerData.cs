using MessagePack;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class MarkerData : OwnerQuantityObjectList<MarkerStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public long GetQuantity(long markerId)
        {
            return Get(markerId).Quantity;
        }
    }

    [MessagePackObject]
    public class MarkerStatus : OwnerQuantityChild
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public override long IdKey { get; set; }
        [Key(3)] public override long Quantity { get; set; }

    }

    public class MarkerDto : OwnerDtoList<MarkerData, MarkerStatus> { }

    public class MarkerDataLoader : OwnerIdDataLoader<MarkerData, MarkerStatus> { }

    public class MarkerDataMapper : OwnerDataMapper<MarkerData, MarkerStatus, MarkerDto> { }
}

using MessagePack;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;

namespace Genrpg.Shared.Currencies.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class CurrencyData : OwnerQuantityObjectList<CurrencyStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public long GetQuantity(long currencyTypeId)
        {
            return Get(currencyTypeId).Quantity;
        }
    }

    [MessagePackObject]
    public class CurrencyStatus : OwnerQuantityChild
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public override long IdKey { get; set; }
        [Key(3)] public override long Quantity { get; set; }

    }

    public class CurrencyDto : OwnerDtoList<CurrencyData, CurrencyStatus> { }

    public class CurrencyDataLoader : OwnerIdDataLoader<CurrencyData, CurrencyStatus> { }



    public class CurrencyDataMapper : OwnerDataMapper<CurrencyData, CurrencyStatus, CurrencyDto> { }
}

using MessagePack;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.Purchasing.PlayerData;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;

namespace OxDb.SharedGame.Currencies.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class CharCurrencyData : NoChildIndexedUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIdLongCollection Data { get; set; } = new SmallIdLongCollection();
        [Key(2)] public override string VersionTag { get; set; }

    }

    public class CharCurrencyLoader : UnitDataLoader<CharCurrencyData> { }
    [MessagePackObject]
    public class CharCurrencyDto : NoChildPlayerDataDto<CharCurrencyData>
    {
        [Key(0)] public override CharCurrencyData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class CharCurrencyDataMapper : NoChildUnitDataMapper<CharCurrencyData, CharCurrencyDto> { }
}



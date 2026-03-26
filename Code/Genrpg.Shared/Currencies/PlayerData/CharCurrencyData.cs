using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;

namespace Genrpg.Shared.Currencies.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class CharCurrencyData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIdLongCollection Data { get; set; } = new SmallIdLongCollection();

    }

    [MessagePackObject]
    public class CurrencyDto : NoChildPlayerDataDto<CharCurrencyData>
    {
        [Key(0)] public override CharCurrencyData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class CurrencyDataMapper : NoChildUnitDataMapper<CharCurrencyData, CurrencyDto> { }
}



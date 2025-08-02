using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Currencies.Settings
{
    [MessagePackObject]
    public class CurrencySettings : ParentSettings<CurrencyType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CurrencyType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }

    }

    public class CurrencySettingsDto : ParentSettingsDto<CurrencySettings, CurrencyType> { }

    public class CurrencySettingsLoader : ParentSettingsLoader<CurrencySettings, CurrencyType> { }

    public class CurrencySettingsMapper : ParentSettingsMapper<CurrencySettings, CurrencyType, CurrencySettingsDto> { }


    public class CurrencyHelper : BaseEntityHelper<CurrencySettings, CurrencyType>
    {
        public override long Key => EntityTypes.Currency;
    }

}

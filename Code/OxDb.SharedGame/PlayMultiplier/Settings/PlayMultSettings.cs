using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.PlayMultiplier.Settings
{

    public class PlayMultSettings : NoChildSettings
    {
        public override string Id { get; set; }
        public double ExtraDailyDistPerTotalCurrencySpend { get; set; }
        public int MaxPlayMult { get; set; }
        public double MaxMultAsAPercentOfCurrentCurrency { get; set; }
    }

    public class PlayMultSettingsDto : NoChildSettingsDto<PlayMultSettings>
    {
        public override PlayMultSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PlayMultSettingsLoader : NoChildSettingsLoader<PlayMultSettings> { }

    public class PlayMultSettingsMapper : NoChildSettingsMapper<PlayMultSettings, PlayMultSettingsDto> { }

}



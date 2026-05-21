using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Trader.Camping.Settings
{

    namespace OxDb.SharedGame.Trader.Camping.Settings
    {
        public class CampingSettings : NoChildSettings // No List
        {
            public override string Id { get; set; }
            public long RoadRegenHours { get; set; }
            public long CityRegenHours { get; set; }
        }


        public class CampingSettingsLoader : NoChildSettingsLoader<CampingSettings> { }


        public class CampingSettingsDto : NoChildSettingsDto<CampingSettings>
        {
            public override CampingSettings Parent { get; set; }
            public override string Id { get; set; }
        }

        public class CampingSettingsMapper : NoChildSettingsMapper<CampingSettings, CampingSettingsDto> { }
    }



}

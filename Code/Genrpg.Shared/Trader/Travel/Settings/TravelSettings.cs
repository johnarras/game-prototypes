using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Trader.Travel.Settings
{
    public class TravelSettings : NoChildSettings
    {
        public override string Id { get; set; }
        public double DistancePerMapUnit { get; set; }
        public long MaxDistanceToTarget { get; set; }
        public long MaxNearbyCitiesShown { get; set; }

        public double BaseForageChance { get; set; }
        public double BaseGoodEventChance { get; set; }
        public double BaseBadEventChance { get; set; }

        public long PortalDistancePerMana { get; set; }
        public long MinPortalCost { get; set; }


    }


    public class TravelSettingsLoader : NoChildSettingsLoader<TravelSettings> { }


    public class TravelSettingsDto : NoChildSettingsDto<TravelSettings>
    {
        public override TravelSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TravelSettingsMapper : NoChildSettingsMapper<TravelSettings, TravelSettingsDto> { }
}

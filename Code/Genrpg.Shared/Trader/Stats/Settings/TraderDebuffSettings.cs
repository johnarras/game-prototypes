using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Stats.Settings
{
    public class TraderDebuffSettings : ParentSettings<TraderDebuff>
    {
        public override string Id { get; set; }
    }

    public class TraderDebuff : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long CleanseCoreCurrencyTypeId { get; set; }
        
        public long CleanseQuantity { get; set; }

        public long CityCoinsCleanseCost { get; set; }

        public List<DebuffEffect> Effects { get; set; } = new List<DebuffEffect>(); 
    }

    public class DebuffEffect : IEffect
    {
        public long EntityTypeId { get; set; }

        public long Quantity { get; set; }

        public long EntityId { get; set; }

    }


    public class TraderDebuffSettingsDto : ParentSettingsDto<TraderDebuffSettings, TraderDebuff>
    {
        public override List<TraderDebuff> Children { get; set; }
        public override TraderDebuffSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TraderDebuffSettingsLoader : ParentSettingsLoader<TraderDebuffSettings, TraderDebuff> { }

    public class TraderDebuffSettingsMapper : ParentSettingsMapper<TraderDebuffSettings, TraderDebuff, TraderDebuffSettingsDto> { }

    public class TraderDebuffEntityHelper : BaseEntityHelper<TraderDebuffSettings, TraderDebuff>
    {
        public override long HelperKey => EntityTypes.TraderDebuff;
    }
}



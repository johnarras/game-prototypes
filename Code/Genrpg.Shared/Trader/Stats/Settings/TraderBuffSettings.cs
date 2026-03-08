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
    public class TraderBuffSettings : ParentSettings<TraderBuff>
    {
        public override string Id { get; set; }
    }

    public class TraderBuff : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public List<BuffEffect> Effects { get; set; } = new List<BuffEffect>(); 
    }

    public class BuffEffect : IEffect
    {
        public long EntityTypeId { get; set; }

        public long Quantity { get; set; }

        public long EntityId { get; set; }

    }


    public class TraderBuffSettingsDto : ParentSettingsDto<TraderBuffSettings, TraderBuff>
    {
        public override List<TraderBuff> Children { get; set; }
        public override TraderBuffSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TraderBuffSettingsLoader : ParentSettingsLoader<TraderBuffSettings, TraderBuff> { }

    public class TraderBuffSettingsMapper : ParentSettingsMapper<TraderBuffSettings, TraderBuff, TraderBuffSettingsDto> { }

    public class TraderBuffEntityHelper : BaseEntityHelper<TraderBuffSettings, TraderBuff>
    {
        public override long HelperKey => EntityTypes.TraderBuff;
    }
}



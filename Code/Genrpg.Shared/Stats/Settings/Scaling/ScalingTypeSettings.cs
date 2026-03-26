using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Stats.Settings.Scaling
{
    public class ScalingType : ChildSettings, IIndexedGameItem
    {


        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Prefix { get; set; }
        public string Art { get; set; }

        public long CrafterTypeId { get; set; }

        public int AttPct { get; set; }
        public int DefPct { get; set; }
        public int OtherPct { get; set; }

        /// <summary>
        /// Used when calculating buy/sell costs
        /// </summary>
        public int CostPct { get; set; }

        public List<StatPct> AddStats { get; set; }

        public long BaseItemTypeId { get; set; }
        /// <summary>
        /// Used for Crawler
        /// </summary>
        public int ArmorPct { get; set; }

        public long MainStatTypeId { get; set; }

        public ScalingType()
        {
            AddStats = new List<StatPct>();
        }
    }

    /// <summary>
    /// This is used to list required Base reagents for crafting. It's a 
    /// percent so it scales according to the recipe core cost.
    /// </summary>
    public class ItemPct
    {
        public long ItemTypeId { get; set; }
        public int Percent { get; set; }
    }

    public class ScalingTypeSettings : ParentSettings<ScalingType>
    {
        public override string Id { get; set; }
    }

    public class ScalingTypeSettingsDto : ParentSettingsDto<ScalingTypeSettings, ScalingType>
    {
        public override List<ScalingType> Children { get; set; }
        public override ScalingTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ScalingTypeSettingsLoader : ParentSettingsLoader<ScalingTypeSettings, ScalingType> { }

    public class ScalingTypeSettingsMapper : ParentSettingsMapper<ScalingTypeSettings, ScalingType, ScalingTypeSettingsDto> { }


    public class ScalingHelper : BaseEntityHelper<ScalingTypeSettings, ScalingType>
    {
        public override long HelperKey => EntityTypes.Scaling;
    }

}



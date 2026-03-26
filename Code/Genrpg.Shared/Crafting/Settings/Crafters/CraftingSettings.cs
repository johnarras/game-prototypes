using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Settings.Crafters
{
    public class CraftingSettings : ParentSettings<CrafterType>
    {
        public override string Id { get; set; }
        public int LootLevelIncrement { get; set; }
        public int PointsPerLevel { get; set; }
        public int PointsPerCraft { get; set; }
        public int ExtraCraftLevelsAllowed { get; set; }
        public int LevelsPerExtraEffect { get; set; }
        public int MaxExtraEffects { get; set; }


        public CraftingSettings()
        {
            LootLevelIncrement = 25;
            PointsPerLevel = 5;
            PointsPerCraft = 1;
            ExtraCraftLevelsAllowed = 5;
            LevelsPerExtraEffect = 4;
            MaxExtraEffects = 5;
        }
    }
    public class CrafterType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public string MousePointer { get; set; }

        public long ReagentItemTypeId { get; set; }

        public float GatherSeconds { get; set; }
        public float CraftingSeconds { get; set; }
        public string GatherActionName { get; set; }
        public string CraftActionName { get; set; }
        public string GatherAnimation { get; set; }
        public string CraftAnimation { get; set; }
    }

    public class CrafterSettingsDto : ParentSettingsDto<CraftingSettings, CrafterType>
    {
        public override List<CrafterType> Children { get; set; }
        public override CraftingSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrafterSettingsLoader : ParentSettingsLoader<CraftingSettings, CrafterType> { }

    public class CraftingSettingsMapper : ParentSettingsMapper<CraftingSettings, CrafterType, CrafterSettingsDto> { }



    public class CrafterSettingsHelper : BaseEntityHelper<CraftingSettings, CrafterType>
    {
        public override long HelperKey => EntityTypes.Crafter;
    }



}



using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crafting.Settings.Recipes
{
    public class Reagent
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public int Quantity { get; set; }

        public string Name { get; set; }
    }
    public class RecipeScaling
    {
        public long ScalingTypeId { get; set; }
    }
    public class RecipeSettings : ParentSettings<RecipeType>
    {
        public override string Id { get; set; }
        public int LootLevelIncrement { get; set; }
        public int PointsPerLevel { get; set; }
        public int PointsPerCraft { get; set; }
        public int ExtraCraftLevelsAllowed { get; set; }
        public int LevelsPerExtraEffect { get; set; }
        public int MaxExtraEffects { get; set; }
        /// <summary>
        /// this is 2.5 meaning each 2.5pct of scaling for the recipe requires 1 reagent in all slots.
        /// </summary>
        public double ReagentQuantityPerPercent { get; set; } = 0.025;

    }

    public class RecipeType : ChildSettings, IIndexedGameItem
    {

        public const string RecipeItemName = "Recipe";


        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public long EntityId { get; set; }
        public long EntityTypeId { get; set; }
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;
        public string Art { get; set; }
        public int ScalingPct { get; set; } = 100;


        /// <summary>
        /// Use this for recipes that have a list of reagents rather than a choice.
        /// </summary>
        public long CrafterTypeId { get; set; }


        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }


        public List<Reagent> ExplicitReagents { get; set; } = new List<Reagent>();

    }


    public class RecipeSettingsDto : ParentSettingsDto<RecipeSettings, RecipeType>
    {
        public override List<RecipeType> Children { get; set; }
        public override RecipeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RecipeSettingsLoader : ParentSettingsLoader<RecipeSettings, RecipeType> { }

    public class RecipeSettingsMapper : ParentSettingsMapper<RecipeSettings, RecipeType, RecipeSettingsDto> { }


    public class RecipeHelper : BaseEntityHelper<RecipeSettings, RecipeType>
    {
        public override long HelperKey => EntityTypes.Recipe;
    }
}



using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Trees
{




    public class TreeTypeSettings : ParentSettings<TreeType>
    {
        public override string Id { get; set; }


        public float TallChance { get; set; } = 0.5f;
        public float TreeDirtRadius { get; set; } = 9.0f;
    }
    public class TreeFlags
    {
        public const int NoNearbyItems = 1 << 2;
        public const int DirectPlaceObject = 1 << 3;
    }

    public class TreeType : ChildSettings, IVariationIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public float Scale { get; set; } = 1.0f;

        public int VariationCount { get; set; } = 1;

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }
        public TreeType()
        {
        }
    }
    public class TreeTypeSettingsDto : ParentSettingsDto<TreeTypeSettings, TreeType>
    {
        public override List<TreeType> Children { get; set; }
        public override TreeTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class TreeTypeSettingsLoader : ParentSettingsLoader<TreeTypeSettings, TreeType> { }

    public class TreeSettingsMapper : ParentSettingsMapper<TreeTypeSettings, TreeType, TreeTypeSettingsDto> { }

    public class TreeEntityHelper : BaseEntityHelper<TreeTypeSettings, TreeType>
    {
        public override long HelperKey => EntityTypes.Tree;
    }
}



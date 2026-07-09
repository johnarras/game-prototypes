using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Trees
{




    public class BushTypeSettings : ParentSettings<BushType>
    {
        public override string Id { get; set; }


        public float TallChance { get; set; } = 0.5f;
        public float TreeDirtRadius { get; set; } = 9.0f;
    }
    public class BushFlags
    {
        public const int IsWaterItem = 1 << 1;
        public const int NoNearbyItems = 1 << 2;
        public const int DirectPlaceObject = 1 << 3;
    }

    public class BushType : ChildSettings, IVariationIndexedGameItem
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
        public BushType()
        {
        }
    }
    public class BushTypeSettingsDto : ParentSettingsDto<BushTypeSettings, BushType>
    {
        public override List<BushType> Children { get; set; }
        public override BushTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class BushTypeSettingsLoader : ParentSettingsLoader<BushTypeSettings, BushType> { }

    public class BushTypeSettingsMapper : ParentSettingsMapper<BushTypeSettings, BushType, BushTypeSettingsDto> { }

    public class BushTypeEntityHelper : BaseEntityHelper<BushTypeSettings, BushType>
    {
        public override long HelperKey => EntityTypes.Bush;
    }
}



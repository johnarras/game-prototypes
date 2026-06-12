using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils.Data;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Plants
{

    public class PlantFlags
    {
        public const int SmallPatches = 1 << 0;
        public const int UsePrefab = 1 << 1;
    }


    /// <summary>
    /// Plants found on the ground used in Unity's grass terrain generator
    /// </summary>

    public class PlantType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public string Art { get; set; }

        public float MinScale { get; set; }
        public float MaxScale { get; set; }

        public MyColorF BaseColor { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public PlantType()
        {
            MinScale = 1.0f;
            MaxScale = 1.0f;

            BaseColor = new MyColorF();
        }

    }
    public class PlantTypeSettings : ParentSettings<PlantType>
    {
        public override string Id { get; set; }
    }

    public class PlantTypeSettingsDto : ParentSettingsDto<PlantTypeSettings, PlantType>
    {
        public override List<PlantType> Children { get; set; }
        public override PlantTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PlantTypeSettingsLoader : ParentSettingsLoader<PlantTypeSettings, PlantType> { }

    public class PlantSettingsMapper : ParentSettingsMapper<PlantTypeSettings, PlantType, PlantTypeSettingsDto> { }

    public class PlantEntityHelper : BaseEntityHelper<PlantTypeSettings, PlantType>
    {
        public override long HelperKey => EntityTypes.Plant;
    }

}



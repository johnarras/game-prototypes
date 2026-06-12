using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils.Data;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Rocks
{
    /// <summary>
    /// Plants found on the ground used in Unity's grass terrain generator
    /// </summary>

    public class RockType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public string Art { get; set; }

        public float ChanceScale { get; set; }

        public int MaxPerZone { get; set; }

        public MyColorF BaseColor { get; set; }

        public int MaxIndex { get; set; }

        public RockType()
        {
            ChanceScale = 1.0f;

            MaxPerZone = 0;
            BaseColor = new MyColorF();

            MaxIndex = 1;
        }

    }
    public class RockTypeSettings : ParentSettings<RockType>
    {
        public override string Id { get; set; }
    }

    public class RockTypeSettingsDto : ParentSettingsDto<RockTypeSettings, RockType>
    {
        public override List<RockType> Children { get; set; }
        public override RockTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RockTypeSettingsLoader : ParentSettingsLoader<RockTypeSettings, RockType> { }

    public class RockSettingsMapper : ParentSettingsMapper<RockTypeSettings, RockType, RockTypeSettingsDto> { }

    public class RockEntityHelper : BaseEntityHelper<RockTypeSettings, RockType>
    {
        public override long HelperKey => EntityTypes.Rock;
    }


}



using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.GroundObjects.Settings
{

    public class GroundObjType : ChildSettings, IIndexedGameItem
    {
        public const string ChestGroup = "chest";
        public const string MineralGroup = "mineral";
        public const string HerbGroup = "herb";
        public const string WoodGroup = "wood";

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string GroupId { get; set; }
        public int SpawnWeight { get; set; }
        public long CrafterTypeId { get; set; }
        public long SpawnTableId { get; set; }
        public int MinRolls { get; set; }
        public int MaxRolls { get; set; }
        public long QualityTypeId { get; set; }
        public bool OneTimeOnly { get; set; }


        public static int GetPositionHash(int x, int y)
        {
            return x / 16 + y / 16 << 10;
        }

    }
    public class GroundObjTypeSettings : ParentSettings<GroundObjType>
    {
        public override string Id { get; set; }
    }

    public class GroundObjTypeSettingsDto : ParentSettingsDto<GroundObjTypeSettings, GroundObjType>
    {
        public override List<GroundObjType> Children { get; set; }
        public override GroundObjTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class GroundObjTypeSettingsLoader : ParentSettingsLoader<GroundObjTypeSettings, GroundObjType> { }

    public class GroundObjSettingsMapper : ParentSettingsMapper<GroundObjTypeSettings, GroundObjType, GroundObjTypeSettingsDto> { }



}



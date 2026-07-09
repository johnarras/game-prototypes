using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;


namespace OxDb.SharedGame.Crawler.MapGen.Settings
{
    public class RoomGenSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public float MinFilledPercent { get; set; }
        public float MaxFilledPercent { get; set; }


        public int MinSize { get; set; }
        public int MaxSize { get; set; }
        public float SquareRoomChance { get; set; }
        public float AxisNoiseChance { get; set; }
        public float UseRoomChance { get; set; }
        public float SizeIncreaseChance { get; set; }
        public float SmallRoomChance { get; set; }
        public float RemoveCellChance { get; set; }
    }

    public class RoomGenSettingsLoader : NoChildSettingsLoader<RoomGenSettings> { }

    public class RoomGenSettingsDto : NoChildSettingsDto<RoomGenSettings>
    {
        public override RoomGenSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class RoomGenSettingsMapper : NoChildSettingsMapper<RoomGenSettings, RoomGenSettingsDto> { }
}
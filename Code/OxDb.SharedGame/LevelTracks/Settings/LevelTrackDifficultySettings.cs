using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.LevelTracks.Settings
{
    public class LevelTrackDifficultySettings : NoChildSettings // No List
    {
        public override string Id { get; set; }

        public long ConstantExp { get; set; }
        public double LinearExpScale { get; set; }

        public double QuadraticExpScale { get; set; }

        public int GetExpToNextLevel(long currentLevel)
        {
            int total = (int)(ConstantExp + currentLevel * LinearExpScale + currentLevel * currentLevel * QuadraticExpScale);
            return total;
        }
    }


    public class LevelTrackDifficultySettingsLoader : NoChildSettingsLoader<LevelTrackDifficultySettings> { }

    public class LevelTrackDifficultySettingsDto : NoChildSettingsDto<LevelTrackDifficultySettings>
    {
        public override LevelTrackDifficultySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class LevelTrackDifficultySettingsMapper : NoChildSettingsMapper<LevelTrackDifficultySettings, LevelTrackDifficultySettingsDto> { }
}



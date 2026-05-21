using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.AI.Settings
{
    public class AISettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public float UpdateSeconds { get; set; } = 1.5f;

        public float IdleWanderChance { get; set; } = 0.25f;

        public float EnemyScanDistance { get; set; } = 20.0f;

        public float LeashDistance { get; set; } = 60.0f;

        public float BaseUnitSpeed { get; set; } = 5.0f;

        public float BringAFriendRadius { get; set; } = 20.0f;
    }


    public class AISettingsLoader : NoChildSettingsLoader<AISettings> { }

    public class AISettingsDto : NoChildSettingsDto<AISettings>
    {
        public override AISettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class AISettingsMapper : NoChildSettingsMapper<AISettings, AISettingsDto> { }
}



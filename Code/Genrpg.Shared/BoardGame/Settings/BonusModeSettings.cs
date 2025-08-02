using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.BoardGame.Settings
{



    [MessagePackObject]
    public class BonusModeSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double DiceMultChargeScaling { get; set; } = 0.5f;
        [Key(2)] public double DiceMultRewardScaling { get; set; } = 0.5f;

    }


    public class BonusModeSettingsLoader : NoChildSettingsLoader<BonusModeSettings> { }


    public class BonusModeSettingsDto : NoChildSettingsDto<BonusModeSettings> { }

    public class BonusModeSettingsMapper : NoChildSettingsMapper<BonusModeSettings, BonusModeSettingsDto> { }


    public class BoardModeHelper : BaseEntityHelper<BoardModeSettings, BoardMode>
    {
        public override long Key => EntityTypes.BoardMode;
    }
}

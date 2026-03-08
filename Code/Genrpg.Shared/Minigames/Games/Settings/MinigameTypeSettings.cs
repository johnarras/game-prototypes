using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;

namespace Genrpg.Shared.Minigames.Games.Settings
{
    public class MinigameTypeSettings : ParentSettings<MinigameType>
    {
        public override string Id { get; set; }
    }

    public class MinigameType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }

        public string ArtSubdirectory { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long MinLevel { get; set; }
        
        public bool Active { get; set; }

        public long WinCoins { get; set; }

        public long LoseCoins { get; set; }
    }

    public class MinigameTypeSettingsDto : ParentSettingsDto<MinigameTypeSettings, MinigameType>
    {
        public override List<MinigameType> Children { get; set; }
        public override MinigameTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class MinigameTypeSettingsLoader : ParentSettingsLoader<MinigameTypeSettings, MinigameType> { }

    public class MinigameTypeSettingsMapper : ParentSettingsMapper<MinigameTypeSettings, MinigameType, MinigameTypeSettingsDto> { }

    public class MinigameTypeEntityHelper : BaseEntityHelper<MinigameTypeSettings, MinigameType>
    {
        public override long HelperKey => EntityTypes.MinigameType;
    }
}



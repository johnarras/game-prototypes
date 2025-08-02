using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Chat.Constants;

namespace Genrpg.Shared.Chat.Settings
{
    [MessagePackObject]
    public class ChatType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public string Color { get; set; }

    }

    [MessagePackObject]
    public class ChatSettings : ParentConstantListSettings<ChatType,ChatTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class ChatSettingsDto : ParentSettingsDto<ChatSettings, ChatType> { }

    public class ChatSettingsLoader : ParentSettingsLoader<ChatSettings, ChatType> { }

    public class ChatSettingsMapper : ParentSettingsMapper<ChatSettings, ChatType, ChatSettingsDto> { }
}

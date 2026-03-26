using Genrpg.Shared.Chat.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Chat.Settings
{
    public class ChatType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string Color { get; set; }

    }

    public class ChatSettings : ParentConstantListSettings<ChatType, ChatTypes>
    {
        public override string Id { get; set; }
    }

    public class ChatSettingsDto : ParentSettingsDto<ChatSettings, ChatType>
    {
        public override List<ChatType> Children { get; set; }
        public override ChatSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ChatSettingsLoader : ParentSettingsLoader<ChatSettings, ChatType> { }

    public class ChatSettingsMapper : ParentSettingsMapper<ChatSettings, ChatType, ChatSettingsDto> { }
}



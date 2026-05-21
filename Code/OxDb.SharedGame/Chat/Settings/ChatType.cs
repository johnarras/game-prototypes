using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Chat.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Chat.Settings
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



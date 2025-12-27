using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Settings.Textures
{
    public class TextureChannel : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string Art { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
    }
    public class TextureChannelSettings : ParentSettings<TextureChannel>
    {
        public override string Id { get; set; }
    }

    public class TextureChannelSettingsDto : ParentSettingsDto<TextureChannelSettings, TextureChannel>
    {
        public override List<TextureChannel> Children { get; set; }
        public override TextureChannelSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class TextureChannelSettingsLoader : ParentSettingsLoader<TextureChannelSettings, TextureChannel> { }

    public class TextureChannelSettingsMapper : ParentSettingsMapper<TextureChannelSettings, TextureChannel, TextureChannelSettingsDto> { }


}



using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.ProcGen.Settings.Textures
{
    public class TextureType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public float Size { get; set; }
        public long ParentTextureTypeId { get; set; }


        public TextureType()
        {
        }
    }
    public class TextureTypeSettings : ParentSettings<TextureType>
    {
        public override string Id { get; set; }
    }

    public class TextureTypeSettingsDto : ParentSettingsDto<TextureTypeSettings, TextureType>
    {
        public override List<TextureType> Children { get; set; }
        public override TextureTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class TextureTypeSettingsLoader : ParentSettingsLoader<TextureTypeSettings, TextureType> { }

    public class TextureSettingsMapper : ParentSettingsMapper<TextureTypeSettings, TextureType, TextureTypeSettingsDto> { }


}



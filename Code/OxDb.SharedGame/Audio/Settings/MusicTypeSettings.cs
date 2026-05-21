using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Audio.Settings
{
    public class MusicType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }


        public float RandomizeSeconds { get; set; }
    }
    public class MusicTypeSettings : ParentSettings<MusicType>
    {
        public override string Id { get; set; }
    }

    public class MusicTypeSettingsDto : ParentSettingsDto<MusicTypeSettings, MusicType>
    {
        public override List<MusicType> Children { get; set; }
        public override MusicTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class MusicTypeSettingsLoader : ParentSettingsLoader<MusicTypeSettings, MusicType> { }

    public class MusicSettingsMapper : ParentSettingsMapper<MusicTypeSettings, MusicType, MusicTypeSettingsDto> { }


}



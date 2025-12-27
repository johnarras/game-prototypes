using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Settings.Clutter
{
    public class ClutterType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int NumChoices { get; set; }
    }
    public class ClutterTypeSettings : ParentSettings<ClutterType>
    {
        public override string Id { get; set; }
    }

    public class ClutterTypeSettingsDto : ParentSettingsDto<ClutterTypeSettings, ClutterType>
    {
        public override List<ClutterType> Children { get; set; }
        public override ClutterTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ClutterTypeSettingsLoader : ParentSettingsLoader<ClutterTypeSettings, ClutterType> { }

    public class ClutterSettingsMapper : ParentSettingsMapper<ClutterTypeSettings, ClutterType, ClutterTypeSettingsDto> { }



}



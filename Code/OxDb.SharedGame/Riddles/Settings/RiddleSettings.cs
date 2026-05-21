using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.Riddles.Settings
{

    public class RiddleSettings : ParentSettings<Riddle>
    {
        public override string Id { get; set; }
    }
    public class Riddle : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public bool IsFullyVisibleText { get; set; }
    }

    public class RiddleSettingsDto : ParentSettingsDto<RiddleSettings, Riddle>
    {
        public override List<Riddle> Children { get; set; }
        public override RiddleSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RiddleSettingsLoader : ParentSettingsLoader<RiddleSettings, Riddle> { }

    public class RiddleSettingsMapper : ParentSettingsMapper<RiddleSettings, Riddle, RiddleSettingsDto> { }

}



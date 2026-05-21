using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Ftue.Settings.Steps
{
    public class FtueStep : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        /// <summary>
        /// Description text shown to the player in the main popup
        /// </summary>
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public long PrereqFtueStepId { get; set; }
        public long FtueTriggerId { get; set; }

        public string TriggerName { get; set; }

        // If this exists, open this screen
        public string ActionScreenName { get; set; }

        // If this exists, this is the only button we can click
        public string ActionButtonName { get; set; }

        /// <summary>
        /// Do we show the popup tint or hide the popup
        /// </summary>
        public long FtuePopupTypeId { get; set; }

    }

    public class FtueStepSettings : ParentSettings<FtueStep>
    {
        public override string Id { get; set; }

        public FtueStep FindFtueStep(long ftueTriggerId, string ftueTriggerName)
        {
            return _data.FirstOrDefault(x => x.FtueTriggerId == ftueTriggerId && x.TriggerName == ftueTriggerName);
        }
    }

    public class FtueStepSettingsDto : ParentSettingsDto<FtueStepSettings, FtueStep>
    {
        public override List<FtueStep> Children { get; set; }
        public override FtueStepSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class FtueStepSettingsLoader : ParentSettingsLoader<FtueStepSettings, FtueStep> { }

    public class FtueStepSettingsMapper : ParentSettingsMapper<FtueStepSettings, FtueStep, FtueStepSettingsDto> { }



}



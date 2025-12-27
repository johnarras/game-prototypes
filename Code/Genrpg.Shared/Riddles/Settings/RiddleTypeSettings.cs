using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Riddles.Settings
{

    public class RiddleTypeSettings : ParentConstantListSettings<RiddleType, RiddleTypes>
    {
        public override string Id { get; set; }
    }
    public class RiddleType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double Weight { get; set; }
        public bool IsToggle { get; set; }
        public bool IsObject { get; set; }
    }

    public class RiddleTypeSettingsDto : ParentSettingsDto<RiddleTypeSettings, RiddleType>
    {
        public override List<RiddleType> Children { get; set; }
        public override RiddleTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class RiddleTypeSettingsLoader : ParentSettingsLoader<RiddleTypeSettings, RiddleType> { }

    public class RiddleTypeSettingsMapper : ParentSettingsMapper<RiddleTypeSettings, RiddleType, RiddleTypeSettingsDto> { }

    public class RiddleTypeEntityHelper : BaseEntityHelper<RiddleTypeSettings, RiddleType>
    {
        public override long HelperKey => EntityTypes.RiddleType;
    }

}



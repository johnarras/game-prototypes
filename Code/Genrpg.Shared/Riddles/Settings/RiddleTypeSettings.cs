using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Utils;
using MessagePack;

namespace Genrpg.Shared.Riddles.Settings
{

    [MessagePackObject]
    public class RiddleTypeSettings : ParentConstantListSettings<RiddleType, RiddleTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class RiddleType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public double Weight { get; set; }
        [Key(9)] public bool IsToggle { get; set; }
        [Key(10)] public bool IsObject { get; set; }
    }

    public class RiddleTypeSettingsDto : ParentSettingsDto<RiddleTypeSettings, RiddleType> { }
    public class RiddleTypeSettingsLoader : ParentSettingsLoader<RiddleTypeSettings, RiddleType> { }

    public class RiddleTypeSettingsMapper : ParentSettingsMapper<RiddleTypeSettings, RiddleType, RiddleTypeSettingsDto> { }

    public class RiddleTypeEntityHelper : BaseEntityHelper<RiddleTypeSettings, RiddleType>
    {
        public override long Key => EntityTypes.RiddleType;
    }

}

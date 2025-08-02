using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Linq;
using Genrpg.Shared.UI.Constants;

namespace Genrpg.Shared.UI.Settings
{
    [MessagePackObject]
    public class ScreenName : ChildSettings, IIdName
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }


    }

    [MessagePackObject]
    public class ScreenNameSettings : ParentConstantListSettings<ScreenName,ScreenNames>
    {
        [Key(0)] public override string Id { get; set; }


        public ScreenName Get(string name)
        {
            return _data.FirstOrDefault(x=>x.Name == name);
        }
    }

    public class ScreenNameSettingsDto : ParentSettingsDto<ScreenNameSettings, ScreenName> { }

    public class ScreenNameSettingsLoader : ParentSettingsLoader<ScreenNameSettings, ScreenName> { }

    public class ScreenNameSettingsMapper : ParentSettingsMapper<ScreenNameSettings, ScreenName, ScreenNameSettingsDto> { }

}

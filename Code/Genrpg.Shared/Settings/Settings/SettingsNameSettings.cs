using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Settings;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Settings.Settings
{
    [MessagePackObject]
    public class SettingsName : ChildSettings, IIdName
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }


        public SettingsName()
        {
        }
    }

    [MessagePackObject]
    public class SettingsNameSettings : ParentSettings<SettingsName>
    {
        [Key(0)] public override string Id { get; set; }

        private Dictionary<string, long> _nameToIdDict = new Dictionary<string, long>();
        public override void SetData(List<SettingsName> data)
        {
            base.SetData(data);

            Dictionary<string, long> tempDict = new Dictionary<string, long>();
            foreach (SettingsName sn in data)
            {
                tempDict[sn.Name] = sn.IdKey;
            }
            _nameToIdDict = tempDict;
        }

        public long GetIdFromTypeName(string typeName)
        {
            if (_nameToIdDict.TryGetValue(typeName, out long id))
            {
                return id;
            }
            return 0;
        }
    }

    public class SettingsNameSettingsDto : ParentSettingsDto<SettingsNameSettings, SettingsName> { }

    public class SettingsNameSettingsLoader : ParentSettingsLoader<SettingsNameSettings, SettingsName> { }

    public class SettingsNameSettingsMapper : ParentSettingsMapper<SettingsNameSettings, SettingsName, SettingsNameSettingsDto> { }

    public class ScreenNameEntityHelper : BaseEntityHelper<ScreenNameSettings, ScreenName>
    {
        public override long HelperKey => EntityTypes.ScreenName;
    }

}

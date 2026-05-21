using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.SettingsNames.Settings
{
    public class SettingsName : ChildSettings, IIdName
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }



        public SettingsName()
        {
        }
    }

    public class SettingsNameSettings : ParentSettings<SettingsName>
    {
        public override string Id { get; set; }

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

    public class SettingsNameSettingsDto : ParentSettingsDto<SettingsNameSettings, SettingsName>
    {
        public override List<SettingsName> Children { get; set; }
        public override SettingsNameSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class SettingsNameSettingsLoader : ParentSettingsLoader<SettingsNameSettings, SettingsName> { }

    public class SettingsNameSettingsMapper : ParentSettingsMapper<SettingsNameSettings, SettingsName, SettingsNameSettingsDto> { }

    public class SettingsNameSettingsEntityHelper : BaseEntityHelper<SettingsNameSettings, SettingsName>
    {
        public override long HelperKey => EntityTypes.SettingsName;
    }

}



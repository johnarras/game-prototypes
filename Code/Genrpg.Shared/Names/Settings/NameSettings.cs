using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Names.Settings
{
    public class WeightedName : IWeightedItem
    {
        public double Weight { get; set; }
        public bool Ignore { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public WeightedName()
        {
            Weight = 1000;
            Ignore = false;
            Name = "";
            Desc = "";
        }
    }
    public class NameSettings : ParentSettings<NameList>
    {
        public override string Id { get; set; }



        public NameList GetNameList(string nm)
        {
            if (_data == null)
            {
                return null;
            }

            foreach (NameList nl in _data)
            {
                if (nl.ListName == nm)
                {
                    return nl;
                }
            }
            return null;
        }

    }



    public class NameList : ChildSettings
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public override string Name { get; set; }
        public string ListName { get; set; }
        public List<WeightedName> Names { get; set; } = new List<WeightedName>();

    }

    public class NameSettingsDto : ParentSettingsDto<NameSettings, NameList>
    {
        public override List<NameList> Children { get; set; }
        public override NameSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class NameSettingsLoader : ParentSettingsLoader<NameSettings, NameList> { }

    public class ItemSettingsMapper : ParentSettingsMapper<NameSettings, NameList, NameSettingsDto> { }

}



using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Utils;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Stats.Settings.Stats;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class ParentConstantListSettings<TChild, TConstants> : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        public override void SetupForEditor(List<object> saveList)
        {
            if (typeof(TChild).Name == "StatType")
            {
                Console.WriteLine("StatTypes");
            }

            List<NameValue> nameList = ReflectionUtils.GetNumericConstants(typeof(TConstants));

            foreach (NameValue nv in nameList)
            {
                IIdName currType = _data.FirstOrDefault(x => x.IdKey == nv.IdKey);

                if (currType == null)
                {
                    TChild child = new TChild();
                    child.IdKey = nv.IdKey;
                    child.Name = nv.Name;
                    _data.Add(child);
                    saveList.Add(child);
                }
            }

            _data = _data.OrderBy(x => x.IdKey).ToList();
            SetData(_data);
            if (_data.FirstOrDefault(x => x.IdKey == 0) == null)
            {
                _data.Insert(0, new TChild() { IdKey = 0, Name = "None" });
            }
        }
    }
}

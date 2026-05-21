using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Names.Entities;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
{
    public abstract class ParentConstantListSettings<TChild, TConstants> : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        public override void SetupForEditor(List<object> saveList)
        {
            List<NameValue> nameList = ConstantUtils.GetNumericConstants(typeof(TConstants));

            foreach (NameValue nv in nameList)
            {
                IIdName currType = _data.FirstOrDefault(x => x.IdKey == nv.IdKey);

                if (currType == null)
                {
                    TChild child = new TChild();
                    child.IdKey = nv.IdKey;
                    child.Name = nv.Name;

                    if (child is IIndexedGameItem indexedChild)
                    {
                        indexedChild.Icon = nv.Name;
                        indexedChild.Art = nv.Name;
                    }

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



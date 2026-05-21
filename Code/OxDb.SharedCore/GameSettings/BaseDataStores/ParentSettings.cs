using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
{
    public abstract class ParentSettings<TChild> : TopLevelGameSettings, IComplexCopy
        where TChild : ChildSettings, new()
    {
        protected List<TChild> _data { get; set; } = new List<TChild>();
        protected Dictionary<long, TChild> _dict = new Dictionary<long, TChild>();
        public virtual void SetData(List<TChild> data)
        {
            _data = data;
            if (data.Count > 0 && data[0] is IId iTempId)
            {
                List<IId> idList = data.Cast<IId>().Where(x => x.IdKey > 0).ToList();
                idList = idList.OrderBy(x => x.IdKey).ToList();
                _data = idList.Cast<TChild>().ToList();
            }
            ClearIndex();
        }

        public override void ClearIndex()
        {

            Dictionary<long, TChild> newDict = new Dictionary<long, TChild>();
            List<TChild> data = _data;
            foreach (TChild child in data)
            {
                if (child is IIdName idname)
                {
                    newDict[idname.IdKey] = child;
                }
            }
            _dict = newDict;
        }

        public override ITopLevelSettings Unpack() { return this; }

        public IReadOnlyList<TChild> GetData() { return _data; }

        public virtual TChild Get(long idkey)
        {
            if (idkey > 0 && _dict.TryGetValue(idkey, out TChild child))
            {
                return child;
            }
            return default;
        }

        public override void SetInternalIds()
        {

            string childLowerName = StrUtils.NormalizeTypeName<TChild>();
            for (int c = 0; c < _data.Count; c++)
            {
                TChild child = _data[c];

                string oldParentId = child.ParentId;
                child.ParentId = Id;

                string childId = child.Id;
                if (child is IId iid)
                {
                    childId = childLowerName + iid.IdKey;
                }
                else
                {
                    if (string.IsNullOrEmpty(childId))
                    {
                        childId = HashUtils.NewGuid().ToString();
                    }
                    if (childId.IndexOf(childLowerName) < 0)
                    {
                        childId = childLowerName + childId;
                    }
                }
                childId = childId.ToLower();
                child.Id = childId;

                if (!string.IsNullOrEmpty(oldParentId))
                {
                    child.Id = child.Id.Replace(oldParentId, "");
                }
                child.Id = child.Id.Replace(Id, "");

                child.Id += Id;
            }
        }

        public override List<IGameSettings> GetChildren() { return new List<IGameSettings>(_data); }

        public void DeepCopyFrom(IComplexCopy from, ISerializer serializer)
        {
            if (from.GetType() == GetType())
            {
                Id = "copy" + DateTime.UtcNow.Ticks % 1000000;
                List<TChild> fromChildren = from.GetDeepCopyData() as List<TChild>;
                if (fromChildren != null)
                {
                    List<TChild> newData = new List<TChild>();
                    foreach (TChild fromChild in fromChildren)
                    {
                        newData.Add(serializer.MakeCopy(fromChild));
                    }
                    SetData(newData);
                    SetInternalIds();
                }
            }
        }

        public object GetDeepCopyData()
        {
            return _data;
        }


        public override void SetupForEditor(List<object> saveList)
        {

            if (typeof(TChild).GetInterface(typeof(IIdName).Name) == null)
            {
                return;
            }
            List<IIdName> idList = _data.Cast<IIdName>().ToList();

            IIdName zeroElement = idList.FirstOrDefault(x => x.IdKey == 0);

            if (zeroElement != null)
            {
                return;
            }

            zeroElement = (IIdName)(Activator.CreateInstance(typeof(TChild)));

            zeroElement.IdKey = 0;
            zeroElement.Name = "None";

            TChild zeroChild = (TChild)zeroElement;

            _data.Insert(0, zeroChild);
        }
    }
}



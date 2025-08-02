using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild
{
    public abstract class OwnerIdObjectList<TChild> : OwnerObjectList<TChild> where TChild : OwnerPlayerData, IId, new()
    {
        protected object _dataLock = new object();

        protected Dictionary<long, TChild> _lookup = new Dictionary<long, TChild>();

        virtual protected bool CreateMissingChildOnGet() { return true; }

        virtual protected void OnCreateChild(TChild newChild)
        {

        }

        protected void SetupLookup(List<TChild> data, bool forceReset)
        {
            if (!forceReset && _lookup != null)
            {
                return;
            }

            Dictionary<long, TChild> dict = new Dictionary<long, TChild>();
            foreach (TChild child in data)
            {
                dict[child.IdKey] = child;
            }
            _lookup = dict;
        }

        public override void SetData(List<TChild> data)
        {
            base.SetData(data);
            SetupLookup(data, true);
        }

        public TChild Get(long id)
        {
            SetupLookup(_data, false);

            if (_lookup.TryGetValue(id, out TChild child))
            {
                return child;
            }

            if (child == null && CreateMissingChildOnGet())
            {
                lock (_dataLock)
                {
                    if (_lookup.TryGetValue(id, out TChild child2))
                    {
                        return child2;
                    }
                    child = _data.FirstOrDefault(x => x.IdKey == id);
                    if (child == null)
                    {
                        child = new TChild()
                        {
                            Id = HashUtils.NewUUId(),
                            OwnerId = Id,
                            IdKey = id,
                        };
                        OnCreateChild(child);
                        _data = new List<TChild>(_data) { child };
                        _lookup[child.IdKey] = child;
                    }
                }
            }
            return child;
        }
    }
}

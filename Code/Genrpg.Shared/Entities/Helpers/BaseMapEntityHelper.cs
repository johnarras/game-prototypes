using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Genrpg.Shared.Entities.Helpers
{
    public abstract class BaseMapEntityHelper<TObj> : IEntityHelper where TObj: IIdName
    {

        protected IMapProvider _mapProvider;

        public abstract long Key { get; }

        public IIdName Find(IFilteredObject obj, long id)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Zones == null)
            {
                return null;
            }

            return _mapProvider.GetMap().GetEditorListFromEntityTypeId(Key).FirstOrDefault();
        }

        public List<IIdName> GetChildList(IFilteredObject obj)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Zones == null)
            {
                return null;
            }

            return _mapProvider.GetMap().GetEditorListFromEntityTypeId(Key);
        }

        public virtual string GetIconAtlasName(IFilteredObject filteredObj, long entityId) 
        {
            IIdName idname = _mapProvider.GetMap().GetEditorListFromEntityTypeId(Key).FirstOrDefault();

            if (idname is IIndexedGameItem indexedItem && !string.IsNullOrEmpty(indexedItem.AtlasPrefix))
            {
                return indexedItem.AtlasPrefix + typeof(TObj).Name + "Icons";
            }


            return typeof(TObj).Name + "Icons"; 
        }

        public string GetEditorPropertyName()
        {
            return typeof(TObj).Name;
        }

        public virtual bool IsMapEntity() { return true; }

        public bool IsEntityHelperFor(string name)
        {
            return (typeof(TObj).Name.ToLower() == name);
        }

        public Type GetParentType()
        {
            return typeof(TObj);
        }

        public Type GetChildType()
        {
            return typeof(TObj);    
        }
    }
}

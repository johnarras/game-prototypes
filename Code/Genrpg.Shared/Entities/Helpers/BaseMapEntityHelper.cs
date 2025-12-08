using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Entities.Helpers
{
    public abstract class BaseMapEntityHelper<TObj> : IEntityHelper where TObj : IIdName
    {

        protected IMapProvider _mapProvider;

        public abstract long HelperKey { get; }

        public IIdName Find(IFilteredObject obj, long id)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Zones == null)
            {
                return null;
            }

            return _mapProvider.GetMap().GetEditorListFromEntityTypeId(HelperKey).FirstOrDefault();
        }

        public List<IIdName> GetChildList(IFilteredObject obj)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Zones == null)
            {
                return null;
            }

            return _mapProvider.GetMap().GetEditorListFromEntityTypeId(HelperKey);
        }

        public virtual string GetIconAtlasName(IFilteredObject filteredObj, long entityId)
        {
            IIdName idname = _mapProvider.GetMap().GetEditorListFromEntityTypeId(HelperKey).FirstOrDefault();

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

        private string _typeNameLowercase = typeof(TObj).Name.ToLower();
        public bool IsEntityHelperFor(string name)
        {
            return StrUtils.IsLowercaseEqual(name, _typeNameLowercase);
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

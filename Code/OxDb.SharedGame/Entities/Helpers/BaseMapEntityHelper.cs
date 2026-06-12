using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Entities.Helpers
{
    public abstract class BaseMapEntityHelper<TObj> : IEntityHelper where TObj : IIdName
    {
        public virtual string GetIconSuffix() { return ""; }
        protected IMapProvider _mapProvider = null;

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

        public virtual string GetIconAtlasName(IFilteredObject filteredObj, long entityId, EEntityIconCategories category)
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

        private string _typeNameLowercase = StrUtils.NormalizeTypeName<TObj>();
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



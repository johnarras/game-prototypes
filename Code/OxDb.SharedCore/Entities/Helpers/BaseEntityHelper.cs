using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.Entities.Helpers
{
    public abstract class BaseEntityHelper<TParent, TChild> : IEntityHelper where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        protected IGameData _gameData;
        public IIdName Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<TParent>(obj).Get(id);
        }

        public List<IIdName> GetChildList(IFilteredObject obj)
        {
            return _gameData.Get<TParent>(obj).GetData().Cast<IIdName>().ToList();
        }

        public virtual string GetIconAtlasName(IFilteredObject obj, long entityId, EEntityIconCategories category)
        {
            TChild child = _gameData.Get<TParent>(obj).Get(entityId);

            // Prefixes can be like LargeCurrencyIcons
            string prefix = "";
            if (category != EEntityIconCategories.Default)
            {
                prefix = category.ToString();
            }

            /// Atlas prefix is for stuff large groups of items that need to be split up like cards or 
            /// markers/pieces for your gameplay.
            if (child is IIndexedGameItem indexedItem && !string.IsNullOrEmpty(indexedItem.AtlasPrefix))
            {
                prefix += indexedItem.AtlasPrefix;
            }
            return prefix + typeof(TChild).Name + "Icons";
        }

        public abstract long HelperKey { get; }

        public virtual string GetEditorPropertyName() { return typeof(TChild).Name; }

        public virtual bool IsMapEntity() { return false; }

        private string _parentTypeName = StrUtils.NormalizeTypeName<TParent>();
        private string _childTypeName = StrUtils.NormalizeTypeName<TChild>();

        public virtual bool IsEntityHelperFor(string name)
        {
            return StrUtils.IsLowercaseEqual(_parentTypeName, name) ||
                StrUtils.IsLowercaseEqual(_childTypeName, name);
        }

        public Type GetParentType()
        {
            return typeof(TParent);
        }

        public Type GetChildType()
        {
            return typeof(TChild);
        }
    }
}



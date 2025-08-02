using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Entities.Helpers
{
    public abstract class BaseEntityHelper<TParent,TChild> : IEntityHelper where TParent: ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
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

        public virtual string GetIconAtlasName(IFilteredObject obj, long entityId) 
        {
            TChild child = _gameData.Get<TParent>(obj).Get(entityId);

            if (child is IIndexedGameItem indexedItem && !string.IsNullOrEmpty(indexedItem.AtlasPrefix))
            {
                return indexedItem.AtlasPrefix + typeof(TChild).Name + "Icon";
            }
            return typeof(TChild).Name + "Icons"; 
        }

        public abstract long Key { get; }

        public virtual string GetEditorPropertyName() { return typeof(TChild).Name; }

        public virtual bool IsMapEntity() { return false; }

        public virtual bool IsEntityHelperFor(string name)
        {
            return typeof(TParent).Name.ToLower() == name.ToLower() ||
                typeof(TChild).Name.ToLower() == name.ToLower();
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

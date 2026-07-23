using OxDb.SharedCore.Entities.Assets;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.Entities.Services
{
    public interface IEntityService : IInjectable
    {
        IEntityHelper GetEntityHelper(long entityTypeId);
        IIdName Find(IFilteredObject obj, long entityType, long entityId);
        T Find<T>(IFilteredObject obj, long entityTypeId, long entityId);
        List<IIdName> GetChildList(IFilteredObject obj, long entityTypeId);
        List<IIdName> GetChildList(IFilteredObject obj, string tableName);
        EntityAtlasIcon TryGetEntityIcon(IFilteredObject obj, long entityTypeId, long entityId, string forcedIconName = "",
            EEntityIconCategories category = EEntityIconCategories.Default);
        IEntityHelper GetEntityHelper(string typeName);
    }

    public class EntityService : IEntityService
    {
        private SetupDictionaryContainer<Type, IGameSettingsLoader> _loaders = new SetupDictionaryContainer<Type, IGameSettingsLoader>();
        private SetupDictionaryContainer<long, IEntityHelper> _entityHelpers = new SetupDictionaryContainer<long, IEntityHelper>();
        protected IGameData _gameData = null;

        public IEntityHelper GetEntityHelper(long entityTypeId)
        {
            if (_entityHelpers.TryGetValue(entityTypeId, out IEntityHelper helper))
            {
                return helper;
            }
            return null;
        }
        public IIdName Find(IFilteredObject obj, long entityType, long entityId)
        {
            IEntityHelper helper = GetEntityHelper(entityType);

            if (helper == null)
            {
                return null;
            }

            return helper.Find(obj, entityId);

        }
        public List<IIdName> GetChildList(IFilteredObject obj, long entityTypeId)
        {
            IEntityHelper helper = GetEntityHelper(entityTypeId);
            if (helper != null)
            {
                return helper.GetChildList(obj);
            }

            return new List<IIdName>();
        }

        public List<IIdName> GetChildList(IFilteredObject obj, string tableName)
        {
            Dictionary<long, IEntityHelper> helpers = _entityHelpers.GetDict();

            IEntityHelper helper = helpers.Values.FirstOrDefault(x => x.GetEditorPropertyName() == tableName);

            if (helper != null)
            {
                return helper.GetChildList(obj).OrderBy(x => x.IdKey).ToList();
            }

            IGameSettingsLoader loader = _loaders.GetDict().Values.FirstOrDefault(x => x.GetChildType().Name == tableName);

            if (loader != null)
            {
                List<ITopLevelSettings> levelSettings = _gameData.AllSettings();

                ITopLevelSettings matchingSettings = levelSettings.FirstOrDefault(x => x.GetType() == loader.HelperKey);

                if (matchingSettings != null)
                {
                    return matchingSettings.GetChildren().Cast<IIdName>().ToList();
                }
            }

            return new List<IIdName>();
        }

        public EntityAtlasIcon TryGetEntityIcon(IFilteredObject obj, long entityTypeId, long entityId,
            string forcedIconName = "", EEntityIconCategories category = EEntityIconCategories.Default)
        {
            IEntityHelper helper = GetEntityHelper(entityTypeId);

            if (helper == null)
            {
                return null;
            }

            string atlasName = "";
            string iconName = forcedIconName;

            IIdName idName = helper.Find(obj, entityId);

            atlasName = helper.GetIconAtlasName(obj, entityId, category);

            if (idName is IIndexedGameItem indexedItem)
            {
                if (string.IsNullOrEmpty(iconName))
                {
                    iconName = indexedItem.Icon;
                }
            }
            else if (string.IsNullOrEmpty(forcedIconName))
            {
                return null;
            }


            if (!string.IsNullOrEmpty(helper.GetIconSuffix()))
            {
                iconName = iconName + helper.GetIconSuffix();
            }

            return new EntityAtlasIcon()
            {
                AtlasName = atlasName,
                IconName = iconName,
            };
        }
        public IEntityHelper GetEntityHelper(string typeName)
        {
            foreach (IEntityHelper helper in _entityHelpers.GetDict().Values)
            {
                if (helper.IsEntityHelperFor(typeName))
                {
                    return helper;
                }
            }
            return null;
        }

        public T Find<T>(IFilteredObject obj, long entityTypeId, long entityId)
        {
            IIdName idn = Find(obj, entityTypeId, entityId);

            if (idn is T t)
            {
                return t;
            }
            return default(T)!;
        }
    }
}



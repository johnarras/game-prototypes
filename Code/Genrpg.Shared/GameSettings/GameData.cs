using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Settings.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.GameSettings
{
    public interface IGameData : IInjectable, IExplicitInject
    {
        List<ITopLevelSettings> AllSettings();
        List<ITopLevelSettings> OverrideSettings();
        List<ITopLevelSettings> DescendingTimeOrderedDefaultSettings();
        void ClearIndex();
        void SetupDataDict(bool force);
        T Get<T>(IFilteredObject obj) where T : IGameSettings;
        void Set(ITopLevelSettings settings);
        void AddData(List<ITopLevelSettings> settingsList);
        void CopyFrom(IGameData data);
    }

    [MessagePackObject]
    public class GameData : IGameData
    {
        public const int IdBlockSize = 10000;

        private List<ITopLevelSettings> _allData { get; set; } = new List<ITopLevelSettings>();
        private List<ITopLevelSettings> _overrideSettings { get; set; } = new List<ITopLevelSettings>();

        private List<ITopLevelSettings> _defaultDataDescendingUpdateTimeList { get; set; } = new List<ITopLevelSettings>();

        public List<ITopLevelSettings> AllSettings()
        {
            return _allData;
        }

        public List<ITopLevelSettings> OverrideSettings()
        {
            return _overrideSettings;
        }



        public List<ITopLevelSettings> DescendingTimeOrderedDefaultSettings()
        {
            return _defaultDataDescendingUpdateTimeList;
        }

        public GameData()
        {
        }

        public void CopyFrom(IGameData gameData)
        {
            _allData = gameData.AllSettings();
            ClearIndex();
        }

        public void ClearIndex()
        {
            foreach (IGameSettings settings in _allData)
            {
                settings.ClearIndex();
            }
            _dataDict = null;
        }

        public void SetupDataDict(bool force)
        {
            if (_dataDict == null || force)
            {
                Dictionary<Type, Dictionary<string, IGameSettings>> tempDict = new Dictionary<Type, Dictionary<string, IGameSettings>>();
                List<ITopLevelSettings> allData = _allData;
                List<ITopLevelSettings> overrideSettings = new List<ITopLevelSettings>();
                foreach (ITopLevelSettings data in allData)
                {
                    if (!tempDict.TryGetValue(data.GetType(), out Dictionary<string, IGameSettings> dataDict))
                    {
                        dataDict = new Dictionary<string, IGameSettings>();
                        tempDict.Add(data.GetType(), dataDict);
                    }

                    dataDict[data.Id] = data;

                    if (data.Id != GameDataConstants.DefaultFilename)
                    {
                        overrideSettings.Add(data);
                    }
                }
                _dataDict = tempDict;
                _overrideSettings = overrideSettings;

                _nameSettings = Get<SettingsNameSettings>(null);

                _defaultDataDescendingUpdateTimeList = allData.Where(x => x.Id == GameDataConstants.DefaultFilename)
                    .OrderByDescending(x => x.SaveTime).ToList();
            }
        }

        private Dictionary<Type, Dictionary<string, IGameSettings>> _dataDict = null!;
        private SettingsNameSettings _nameSettings = null!;
        public virtual T Get<T>(IFilteredObject obj) where T : IGameSettings
        {
            SetupDataDict(false);

            string dataName = SettingObjectName<T>(obj);

            if (_dataDict.TryGetValue(typeof(T), out Dictionary<string, IGameSettings> typeDict))
            {
                if (typeDict.TryGetValue(dataName, out IGameSettings data))
                {
                    return (T)data;
                }

                if (typeDict.TryGetValue(GameDataConstants.DefaultFilename, out IGameSettings defaultData))
                {
                    return (T)defaultData;
                }

                return (T)typeDict.Values.FirstOrDefault();
            }

            return default(T)!;
        }

        public void Set(ITopLevelSettings t)
        {
            if (t is IChildSettings childSettings)
            {
                return;
            }

            ITopLevelSettings t1 = t.Unpack();

            ITopLevelSettings currentObject = _allData.FirstOrDefault(x => x.Id == t1.Id && x.GetType() == t1.GetType());
            if (currentObject != null)
            {
                _allData.Remove(currentObject);
                if (_overrideSettings.Contains(currentObject))
                {
                    _overrideSettings.Remove(currentObject);
                }
            }
            _allData.Add(t1);
            if (t1.Id != GameDataConstants.DefaultFilename)
            {
                _overrideSettings.Add(t1);
            }

        }

        public void AddData(List<ITopLevelSettings> settingsList)
        {
            foreach (ITopLevelSettings settings in settingsList)
            {
                Set(settings);
            }
            SetupDataDict(true);
        }

        protected string SettingObjectName<T>(IFilteredObject obj)
        {
            if (obj == null || obj.DataOverrides == null || obj.DataOverrides.Items == null || _nameSettings == null)
            {
                return GameDataConstants.DefaultFilename;
            }

            long settingsId = _nameSettings?.GetIdFromTypeName(typeof(T).Name) ?? 0;

            PlayerSettingsOverrideItem item = obj.DataOverrides.Items.FirstOrDefault(x => x.SettingsNameId == settingsId);

            if (item != null || settingsId == 0 || settingsId == 93)
            {
                Console.WriteLine("Found AB");
            }
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }
    }
}

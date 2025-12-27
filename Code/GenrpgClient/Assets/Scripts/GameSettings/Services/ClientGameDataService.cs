using Assets.Scripts.Assets;
using Assets.Scripts.GameSettings.Entities;
using Assets.Scripts.Repository;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.GameSettings.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameSettings.Services
{
    public interface IClientGameDataService : IGameDataService, IInitializable
    {
        Awaitable SaveSettings(IGameSettings settings);

        Awaitable EditorLoadCachedSettings(IClientGameState gs);

    }
    public class ClientGameDataService : IClientGameDataService
    {

        private IClientRepositoryService _repoService = null!;
        protected IGameData _gameData = null!;
        private IClientAppService _clientAppService = null!;
        private IClientConfigContainer _configContainer = null!;
        private ILocalLoadService _localLoadService = null!;
        private ITextSerializer _serializer = null!;
        private IClientGameState _gs = null;

        private Dictionary<Type, IGameSettingsMapper> _loaderObjects = null;

        protected string GetFullBakedGameDataPath()
        {
            return _clientAppService.DataPath + "/Resources/" + BakedGameDataPathSuffix;
        }

        public async Task Initialize(CancellationToken token)
        {
            List<Type> mapperTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettingsMapper));

            Dictionary<Type, IGameSettingsMapper> newList = new Dictionary<Type, IGameSettingsMapper>();
            foreach (Type lt in mapperTypes)
            {
                if (Activator.CreateInstance(lt) is IGameSettingsMapper newLoader && newLoader.SendToClient())
                {
                    newList[newLoader.GetClientType()] = newLoader;
                }
            }
            _loaderObjects = newList;
            await Task.CompletedTask;
        }

        public async Task<IGameData> LoadGameData()
        {
            await LoadCachedSettingsInternal(_gs, false);
            return _gameData;
        }

        public async Awaitable EditorLoadCachedSettings(IClientGameState gs)
        {
            await LoadCachedSettingsInternal(gs, true);
        }

        const string BakedGameDataPathSuffix = "BakedGameData/";
        public async Awaitable LoadCachedSettingsInternal(IClientGameState gs, bool useBakedSettings)
        {
            GameData gameData = new ClientGameData();

            List<ITopLevelSettings> allSettings = new List<ITopLevelSettings>();
            foreach (IGameSettingsMapper loader in _loaderObjects.Values)
            {
                ITopLevelSettings bakedSettings = null;

                string bakedResourcesPath = BakedGameDataPathSuffix + loader.GetClientType().Name;

                TextAsset textAsset = _localLoadService.LocalLoad<TextAsset>(bakedResourcesPath);

                if (textAsset != null && !string.IsNullOrEmpty(textAsset.text))
                {
                    bakedSettings = (ITopLevelSettings)_serializer.DeserializeWithType(textAsset.text, loader.GetClientType());
                }

                if (!useBakedSettings && (!_configContainer.Config.SelfContainedClient ||
                    _configContainer.Config.ExportGameData))
                {
                    List<ITopLevelSettings> settingsChoices = new List<ITopLevelSettings>();

                    object obj = await _repoService.LoadWithType(loader.GetClientType(), GameDataConstants.DefaultFilename);

                    ITopLevelSettings downloadedSettings = obj as ITopLevelSettings;

                    if (_configContainer.Config.SelfContainedClient &&
                        _configContainer.Config.ExportGameData && bakedSettings != null &&
                        (downloadedSettings == null || (downloadedSettings.SaveTime < bakedSettings.SaveTime)))

                    {
                        await _repoService.Save(bakedSettings, new RepoSaveArgs() { Verbose = true });
                    }

                    // If baked settings are newer than the cached downloaded settings, use the new baked data in place of the cached.
                    // This comes up if you create a new client.
                    if (bakedSettings != null && downloadedSettings != null &&
                        bakedSettings.SaveTime >= downloadedSettings.SaveTime)
                    {
                        downloadedSettings = bakedSettings;
                    }

                    if (downloadedSettings != null)
                    {
                        allSettings.Add(downloadedSettings);
                    }
                    else if (bakedSettings != null)
                    {
                        allSettings.Add(bakedSettings);
                    }
                }
                else
                {
                    allSettings.Add(bakedSettings);
                }

            }
            gameData.AddData(allSettings);
            _gameData.CopyFrom(gameData);

            await Task.CompletedTask;
        }

        public async Awaitable SaveSettings(IGameSettings settings)
        {
            await _repoService.Save(settings);

#if UNITY_EDITOR

            string dirName = GetFullBakedGameDataPath();

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            string path = dirName + settings.GetType().Name + ".txt";

            string serializedData = _serializer.PrettyPrint(settings);

            File.WriteAllText(path, serializedData);
#endif
        }
    }
}



using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Setup;
using Genrpg.Editor.UI.Interfaces;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Serialization.Utils;
using Genrpg.Shared.SettingsNames.Settings;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Editor.Utils
{
    public static class EditorGameDataUtils
    {
        public static async Task<FullGameDataCopy> LoadFullGameData(IUICanvas form, string env, CancellationToken token)
        {
            EditorGameState gs = await SetupFromConfig(form, env, false);

            FullGameDataCopy dataCopy = new FullGameDataCopy();

            IServerGameDataService gameDataService = gs.loc.Get<IServerGameDataService>();
            IFullRepositoryService repoService = gs.loc.Get<IFullRepositoryService>();
            List<IGameSettingsLoader> allLoaders = gameDataService.GetAllLoaders();

            foreach (IGameSettingsLoader loader in allLoaders)
            {
                List<ITopLevelSettings> allSettings = await loader.LoadAll(repoService, true);
                foreach (ITopLevelSettings data in allSettings)
                {
                    dataCopy.Data.Add(data);
                }
            }

            return dataCopy;
        }

        public static async Task<EditorGameState> SetupFromConfig(object parent, string env, bool setupForEditor, IServerConfig serverConfig = null)
        {
            if (serverConfig == null)
            {
                ConfigSetup configSetup = new ConfigSetup();
                serverConfig = await configSetup.SetupServerConfig(EditorGameState.CTS.Token, CloudServerNames.Editor.ToString().ToLower());
            }
            serverConfig.DefaultEnv = env;
            EditorGameState gs = await new ServerSetup().SetupFromConfig<EditorGameState, EditorSetupService>(parent, CloudServerNames.Editor.ToString().ToLower(),
              EditorGameState.CTS.Token, serverConfig);

            gs.data = gs.loc.Get<IGameData>();
            List<ITopLevelSettings> allSettings = gs.data.AllSettings();

            foreach (ITopLevelSettings settings in allSettings)
            {
                if (setupForEditor)
                {
                    settings.SetupForEditor(gs.LookedAtObjects);
                }
                if (settings.SaveTime == DateTime.MinValue)
                {
                    gs.LookedAtObjects.Add(settings);
                }
            }

            return gs;
        }


        public static async Task SaveFullGameData(IUICanvas form, FullGameDataCopy dataCopy, string env, bool deleteExistingData, CancellationToken token)
        {

            try
            {
                EditorGameState gs = await SetupFromConfig(form, env, false);
                IRepositoryService repoService = gs.loc.Get<IRepositoryService>();

                List<IGameSettings> dataList = new List<IGameSettings>();

                // This will overload Cosmos serverless...soo put breakpoints here to slow down the saving
                // to avoid 429 errors
                List<Task> saveTasks = new List<Task>();
                for (int i = 0; i < dataCopy.Data.Count; i++)
                {
                    saveTasks.Add(repoService.Save(dataCopy.Data[i]));

                    if (i % 10 == 9 || i == dataCopy.Data.Count - 1)
                    {
                        await Task.WhenAll(saveTasks);
                        saveTasks = new List<Task>();
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        public static void InitSerialization()
        {
            SerializationInitializer.Init(GetCodeFolderPath());
        }

        static string GetCodeFolderPath() { return AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..\\..\\..\\"; }

        const string GitOffsetPath = "..\\GameData";
        public static void WriteGameDataToDisk(FullGameDataCopy dataCopy, ITextSerializer serializer)
        {
            string dirName = GetCodeFolderPath();

            dirName += GitOffsetPath;

            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName, true);
            }
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            foreach (string file in Directory.GetFiles(dirName))
            {
                File.Delete(file);
            }

            DateTime saveTime = DateTime.UtcNow;
            foreach (IGameSettings data in dataCopy.Data)
            {
                WriteGameDataText(dirName, data, serializer, saveTime);

                foreach (IGameSettings child in data.GetChildren())
                {
                    WriteGameDataText(dirName, child, serializer, saveTime);
                }
                RemoveDeletedFiles(dirName, data.GetChildren());
            }
            RemoveDeletedFiles(dirName, dataCopy.Data);


        }


        public static void WriteGameDataToClient(List<ITopLevelSettings> defaultClientSettings, ITextSerializer serializer)
        {
            string dirName = GetCodeFolderPath() + "..\\Code\\" + Game.Prefix + "Client\\Assets\\Resources\\BakedGameData";

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            foreach (ITopLevelSettings settingsItem in defaultClientSettings)
            {
                string txt = serializer.PrettyPrint(settingsItem);
                string filename = settingsItem.GetType().Name + ".txt";

                File.WriteAllText(dirName + "\\" + filename, txt);
            }
        }


        private static void RemoveDeletedFiles(string parentPath, List<IGameSettings> allSettings)
        {
            if (allSettings.Count < 1)
            {
                return;
            }

            foreach (IGameSettings settings in allSettings)
            {
                string subpath = settings.GetType().Name.ToLower();

                string fullDir = Path.Combine(parentPath, subpath);

                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }

                string[] fileNames = Directory.GetFiles(fullDir);

                foreach (string fileName in fileNames)
                {
                    IGameSettings matchingObject = allSettings.FirstOrDefault(x => x.GetType() == settings.GetType() && x.Id == settings.Id);

                    if (matchingObject == null)
                    {
                        string fullPath = Path.Combine(fullDir, fileName);
                        File.Delete(fullPath);
                    }
                }
            }
        }

        private static void WriteGameDataText(string parentPath, object objectToSave, ITextSerializer serializer,
            DateTime saveTime)
        {
            IStringId idObj = objectToSave as IStringId;

            if (idObj == null)
            {
                return;
            }

            BaseGameSettings baseSettings = idObj as BaseGameSettings;

            if (baseSettings != null)
            {

                ITopLevelSettings topLevel = idObj as ITopLevelSettings;

                if (topLevel == null)
                {
                    baseSettings.SaveTime = DateTime.MinValue;
                }
                else
                {
                    if (MainWindow.UpdateSaveTime)
                    {
                        baseSettings.SaveTime = saveTime;
                    }
                }
            }

            string subpath = objectToSave.GetType().Name.ToLower();

            string fullDir = Path.Combine(parentPath, subpath);

            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }


            string fullPath = Path.Combine(fullDir, idObj.Id);

            string txt = serializer.PrettyPrint(idObj);
            File.WriteAllText(fullPath, txt);
        }

        public static async Task<FullGameDataCopy> LoadDataFromDisk(IUICanvas form, CancellationToken token)
        {

            EditorGameState gs = await SetupFromConfig(form, EnvNames.Dev, false);

            ITextSerializer serializer = gs.loc.Get<ITextSerializer>();

            FullGameDataCopy dataCopy = new FullGameDataCopy();

            List<Type> settingsTypes = ReflectionUtils.GetTypesImplementing(typeof(IGameSettings));

            string mainDirName = GetCodeFolderPath() + GitOffsetPath;

            if (!Directory.Exists(mainDirName))
            {
                Directory.CreateDirectory(mainDirName);
            }

            string[] fullDirectoryNames = Directory.GetDirectories(mainDirName);

            List<string> directoryNames = new List<string>();

            foreach (string fullName in fullDirectoryNames)
            {
                directoryNames.Add(fullName.Replace(mainDirName + "\\", ""));
            }

            foreach (string subDirName in directoryNames)
            {
                Type currType = settingsTypes.FirstOrDefault(x => StrUtils.IsLowercaseEqual(x.Name, subDirName));

                if (currType == null)
                {
                    Console.WriteLine("Unknown IGameSetting type {0}", subDirName);
                    continue;
                }

                try
                {
                    if (subDirName.IndexOf("version") >= 0)
                    {
                        Console.Write("Version settings");
                    }
                    string fullDirectoryName = Path.Combine(mainDirName, subDirName);

                    if (!Directory.Exists(fullDirectoryName))
                    {
                        continue;
                    }

                    string[] fileNames = Directory.GetFiles(fullDirectoryName);

                    List<string> allFiles = new List<string>();

                    foreach (string file in fileNames)
                    {
                        allFiles.Add(File.ReadAllText(Path.Combine(fullDirectoryName, file)));
                    }

                    foreach (string fileData in allFiles)
                    {
                        dataCopy.Data.Add((IGameSettings)serializer.DeserializeWithType(fileData, currType));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message + " " + e.StackTrace);
                }
            }
            return dataCopy;
        }

        public static async Task<EditorGameState> SetupForEditing(WindowBase window, string action, string env, Action<EditorGameState> afterAction = null)
        {
            try
            {
                EditorGameState gs = await EditorGameDataUtils.SetupFromConfig(window, env, true);

                ITextSerializer serializer = gs.loc.Get<ITextSerializer>();

                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


                DateTime buildtime = new DateTime(2000, 1, 1)
                        .AddDays(version.Build).AddSeconds(version.Revision * 2);

                gs.EditorGameData = new EditorGameData()
                {
                    GameData = gs.data
                };

                List<ITopLevelSettings> allGameData = gs.data.AllSettings();

                List<IGrouping<Type, ITopLevelSettings>> groups = allGameData.GroupBy(x => x.GetType()).ToList();

                groups = groups.OrderBy(x => x.Key.Name).ToList();

                SettingsNameSettings settingSettings = (SettingsNameSettings)allGameData.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename && x.GetType().Name == nameof(SettingsNameSettings));

                List<SettingsName> allSettingNames = settingSettings.GetData().ToList();

                long maxIndex = 0;

                if (allSettingNames.Count > 0)
                {
                    maxIndex = allSettingNames.Max(x => x.IdKey);
                }




                AddEntityListData<EntitySettings, EntityType, EntityTypes>(gs);
                AddEntityListData<ScreenNameSettings, ScreenName, ScreenNames>(gs);

                foreach (IGrouping<Type, ITopLevelSettings> group in groups)
                {
                    string typeName = group.Key.Name;

                    SettingsName currName = allSettingNames.FirstOrDefault(x => x.Name == typeName);

                    if (currName == null)
                    {
                        currName = new SettingsName() { Id = GameDataConstants.DefaultFilename, Name = typeName, IdKey = ++maxIndex };
                        allSettingNames.Add(currName);
                        gs.LookedAtObjects.Add(currName);
                    }


                    List<ITopLevelSettings> orderedList = group.OrderBy(x => x.Id).ToList();

                    List<BaseGameSettings> items = new List<BaseGameSettings>();

                    for (int i = 0; i < orderedList.Count; i++)
                    {
                        BaseGameSettings setting = orderedList[i] as BaseGameSettings;
                        if (setting != null)
                        {
                            items.Add(setting);
                            if (setting.SaveTime == DateTime.MinValue)
                            {
                                gs.LookedAtObjects.Add(setting);
                            }
                            foreach (IGameSettings childSetting in setting.GetChildren())
                            {
                                if (childSetting is IUpdateData updateChild)
                                {
                                    if (updateChild.UpdateTime == DateTime.MinValue)
                                    {
                                        gs.LookedAtObjects.Add(updateChild);
                                    }
                                }
                            }
                        }
                    }


                    Type baseCollectionType = typeof(TypedEditorSettingsList<>);
                    Type genericType = baseCollectionType.MakeGenericType(group.Key);
                    EditorSettingsList list = (EditorSettingsList)Activator.CreateInstance(genericType);
                    list.SetData(items);
                    list.TypeName = "[" + group.Count() + "] " + group.Key.Name;
                    gs.EditorGameData.Data.Add(list);
                }

                settingSettings.SetData(allSettingNames);

                if (afterAction != null)
                {
                    afterAction.Invoke(gs);
                }

                window.DispatcherQueue.TryEnqueue(() =>
                {
                    DataWindow win = new DataWindow(gs, gs.EditorGameData, window, action);

                    win.Activate();
                });

                return gs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
            return null;
        }

        private static void AddEntityListData<TParent, TChild, TConstantList>(EditorGameState gs)
            where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
        {
            TParent parent = gs.data.Get<TParent>(null);

            List<IIdName> childList = parent.GetData().Cast<IIdName>().ToList();


            List<NameValue> nameList = ReflectionUtils.GetNumericConstants(typeof(TConstantList));


            foreach (NameValue nv in nameList)
            {
                IIdName currType = childList.FirstOrDefault(x => x.IdKey == nv.IdKey);

                if (currType == null)
                {
                    TChild child = new TChild();
                    child.IdKey = nv.IdKey;
                    child.Name = nv.Name;
                    childList.Add(child);
                    gs.LookedAtObjects.Add(child);
                }
            }

            childList = childList.OrderBy(x => x.IdKey).ToList();

            parent.SetData(childList.Cast<TChild>().ToList());


        }
    }
}



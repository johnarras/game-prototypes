using MongoDB.Bson.Serialization.Serializers;
using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Interfaces;
using OxDb.DataUtils.Services.EditorData;
using OxDb.DataUtils.Services.Setup;
using OxDb.ServerCore.MainServer;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.SettingsNames.Settings;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.Zones.Settings;
using ZstdSharp.Unsafe;

namespace OxDb.DataUtils.Utils
{
    public class EditorDataSetup
    {
        public async Task<EditorServer> SetupEditorServer(IWindowBase window, List<IInjectable> initialServices, string env, bool setupForEditor, string actionName, OnEditorClickAction afterAction)
        {
            try
            {
                EditorServer server = new EditorServer();
                ServerInitArgs args = new ServerInitArgs(initialServices, EditorGameState.CTS.Token, null, null, env);

                await server.Init(args);

                EditorGameState gs = (EditorGameState)server.GetServerGameState();

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

                ITextSerializer serializer = gs.loc.Get<ITextSerializer>();

                gs.EditorGameData = new EditorGameData()
                {
                    GameData = gs.data
                };

                List<ITopLevelSettings> allGameData = gs.data.AllSettings();

                List<IGrouping<Type, ITopLevelSettings>> groups = allGameData.GroupBy(x => x.GetType()).ToList();

                groups = groups.OrderBy(x => x.Key.Name).ToList();

                SettingsNameSettings settingSettings = (SettingsNameSettings)allGameData.FirstOrDefault(x => x.Id == GameDataConstants.DefaultFilename && x.GetType().Name == nameof(SettingsNameSettings));

                if (settingSettings == null)
                {
                    settingSettings = new SettingsNameSettings() { Id = GameDataConstants.DefaultFilename };
                }


                List<SettingsName> allSettingNames = settingSettings.GetData().ToList();

                long maxIndex = 0;

                if (allSettingNames.Count > 0)
                {
                    maxIndex = allSettingNames.Max(x => x.IdKey);
                }
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
                                if (childSetting.SaveTime == DateTime.MinValue)
                                {

                                    if (childSetting is IId idChild && idChild.IdKey == 0)
                                    {
                                        continue;
                                    }
                                    gs.LookedAtObjects.Add(childSetting);
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
                    await afterAction.Invoke(server, gs, gs.loc.Get<IEditorDataService>(), EditorGameState.CTS.Token);
                }


                if (actionName == "Data" || actionName == "Maps")
                {
                    window?.ShowDataWindow(gs, gs.EditorGameData, actionName);
                }
                return server;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
            return null;
        }
    }
}



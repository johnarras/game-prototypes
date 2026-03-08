using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Interfaces;
using Genrpg.DataUtils.Services.EditorData;
using Genrpg.DataUtils.Services.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.SettingsNames.Settings;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Settings;
using Genrpg.Shared.Utils;

namespace Genrpg.DataUtils.Utils
{
    public class EditorDataSetup
    {


        public  async Task<EditorGameState> SetupGameState(IWindowBase window, string env, bool setupForEditor, string actionName,  OnEditorClickAction afterAction)
        {
            try
            {
                ConfigSetup configSetup = new ConfigSetup();
                IServerConfig serverConfig = await configSetup.SetupServerConfig(EditorGameState.CTS.Token, CloudServerNames.Editor.ToString().ToLower(), env);

                serverConfig.DefaultEnv = env;
                EditorGameState gs = await new ServerSetup().SetupFromConfig<EditorGameState, EditorSetupService>(window, CloudServerNames.Editor.ToString().ToLower(),
                  EditorGameState.CTS.Token, serverConfig, env);

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
                    await afterAction.Invoke(gs, gs.loc.Get<IEditorDataService>(), EditorGameState.CTS.Token);
                }


                if (actionName == "Data" || actionName == "Maps")
                {
                    window?.ShowDataWindow(gs, gs.EditorGameData, actionName);
                }
                return gs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
            return null;
        }


        private void AddEntityListData<TParent, TChild, TConstantList>(EditorGameState gs)
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



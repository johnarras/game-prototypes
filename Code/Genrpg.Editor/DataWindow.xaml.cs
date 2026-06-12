using Genrpg.Editor.UI;
using OxDb.DataUtils.Constants;
using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Interfaces;
using OxDb.DataUtils.Services.EditorData;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.GameSettings.Settings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.SettingsNames.Settings;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedGame.Versions.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataWindow : WindowBase, IUICanvas
    {

        protected IServerConfig _serverConfig = null;
        protected IRepositoryService _repoService = null;
        protected IServerGameDataService _gameDataService = null;
        protected ITaskService _taskService = null;
        protected IEditorDataService _dataService = null;
        IReflectionService _reflectionService = null;


        private EditorGameState _gs = null;
        public IList<UserControlBase> ViewStack = null;
        private Object obj = null;
        public String action = "";
        private WindowBase _parentForm;



        int _width = 1920;
        int _height = 1080;

        public int Width => _width;
        public int Height => _height;

        public DataWindow(EditorGameState gsIn, Object objIn, WindowBase parentFormIn, String actionIn)
        {
            _parentForm = parentFormIn;
            _gs = gsIn;
            _gs.loc.Resolve(this);
            action = actionIn;
            ViewStack = new List<UserControlBase>();
            obj = objIn;

            if (obj == null)
            {
                return;
            }

            // 1. Lock your 10-year-old internal field dimensions onto the canvas layout root
            _canvas.Width = this._width;   // 1920
            _canvas.Height = this._height; // 1080

            // 2. Wrap the canvas inside your automatic Viewbox shield
            Microsoft.UI.Xaml.Controls.Viewbox layoutBridge = new Microsoft.UI.Xaml.Controls.Viewbox()
            {
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch
            };

            layoutBridge.Child = _canvas;

            // 3. Hand the Viewbox root directly to the Window Content panel
            Content = layoutBridge;

            // 4. Resize the native desktop frame using your dynamic hardware scaling lookup
            UIHelper.SetWindowRect(this, 50, 50, _width, _height);

            // 5. Run your procedural layout rendering loops safely
            AddView(action);
        }

        private void DataWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
        {
            // Unhook instantly so this logic only executes once on startup
            this.Activated -= DataWindow_Activated;

            // Use your updated internal fields
            _canvas.Width = this._width;   // 1920
            _canvas.Height = this._height; // 1080

            Microsoft.UI.Xaml.Controls.Viewbox layoutBridge = new Microsoft.UI.Xaml.Controls.Viewbox()
            {
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch
            };

            layoutBridge.Child = _canvas;
            Content = layoutBridge;

            // Scale the window using the dynamic hardware lookup
            UIHelper.SetWindowRect(this, 50, 50, _width, _height);

            AddView(action);
        }
        public void AddView(String action)
        {
            UserControlFactory ucf = new UserControlFactory();
            UserControlBase view = null;
            if (action == "Users")
            {
                view = new FindUserView(_gs, this);
            }
            else if (action == "Data")
            {
                view = ucf.Create(_gs, this, obj, null, null, null);
            }
            else if (action == "Map")
            {
                view = ucf.Create(_gs, this, obj, null, null, null);
            }
        }

        public void GoBack()
        {
            if (ViewStack == null || ViewStack.Count < 2)
            {
                return;
            }

            UserControlBase control = ViewStack[ViewStack.Count - 2];
            if (control == null)
            {
                return;
            }

            ViewStack.RemoveAt(ViewStack.Count - 1);

            _canvas.ClearChildren();
            _canvas.Add(control);
            DataView dv = control as DataView;
            if (dv != null)
            {
                dv.ShowData();
            }
        }

        public void GoHome()
        {
            if (ViewStack == null || ViewStack.Count < 2)
            {
                return;
            }

            UserControlBase control = ViewStack[0];
            if (control == null)
            {
                return;
            }

            while (ViewStack.Count > 1)
            {
                ViewStack.RemoveAt(ViewStack.Count - 1);
            }

            _canvas.ClearChildren();
            _canvas.Add(control);
            DataView dv = control as DataView;
            if (dv != null)
            {
                dv.StartTick();
            }
        }

        public void AddChildView(UserControlBase dv)
        {
            _canvas.ClearChildren();
            _canvas.Add(dv);
            ViewStack.Add(dv);
        }

        public void AddControl(object cont, int top = 0, int left = 0)
        {
            _canvas.Add(cont);
        }

        public async Task SaveData(EditorGameState gs, bool copyData)
        {

            String env = _serverConfig.Env;

            if (action == "Data")
            {
                foreach (DataView dataView in ViewStack)
                {
                    if (dataView.Obj is IGameSettings settings &&
                        !gs.LookedAtObjects.Contains(settings))
                    {
                        gs.LookedAtObjects.Add(settings);
                    }
                }

                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                bool foundBadData = false;

                List<IGrouping<Type, ITopLevelSettings>> groups = gs.data.AllSettings().GroupBy(x => x.GetType()).ToList();

                List<ITopLevelSettings> allSettings = gs.data.AllSettings();
                foreach (ITopLevelSettings settings in allSettings)
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        await ShowMessageBox("Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }

                foreach (IGrouping<Type, ITopLevelSettings> group in groups)
                {
                    List<ITopLevelSettings> items = group.ToList();

                    if (items.Count > 0)
                    {
                        if (items[0] is IIdName idn)
                        {
                            List<IIdName> idNameList = items.Cast<IIdName>().ToList();

                            List<IGrouping<long, IIdName>> idkeyGroups = idNameList.GroupBy(x => x.IdKey).ToList();

                            foreach (IGrouping<long, IIdName> idNameGroup in idkeyGroups)
                            {
                                if (idNameGroup.Count() > 1)
                                {
                                    List<IIdName> badIdList = idNameGroup.ToList();
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("Duplicate Idkey in " + badIdList[0].GetType().Name + ": " + badIdList[0].IdKey);
                                    foreach (IIdName idName in badIdList)
                                    {
                                        sb.Append(idName.Name + " ");
                                    }
                                    await ShowMessageBox(sb.ToString());
                                    foundBadData = true;
                                }
                            }

                        }
                    }

                    List<IGrouping<string, ITopLevelSettings>> nameGroups = items.GroupBy(x => x.Id).ToList();

                    if (items.Count != nameGroups.Count)
                    {
                        await ShowMessageBox("Setting " + group.Key.Name + " has duplicate DocId");
                        foundBadData = true;
                    }
                }

                DataOverrideSettings overrideSettings = gs.data.Get<DataOverrideSettings>(null);

                List<DataOverrideGroup> groupList1 = overrideSettings.GetData().ToList();

                List<DataOverrideGroup> groupList2 = new List<DataOverrideGroup>(groupList1);

                StringBuilder overrideSB = new StringBuilder();
                foreach (DataOverrideGroup group1 in groupList1)
                {
                    foreach (DataOverrideGroup group2 in groupList2)
                    {
                        if (group2.EndDate <= group1.StartDate ||
                            group2.StartDate >= group1.EndDate ||
                            group1.Priority != group2.Priority)
                        {
                            continue;
                        }

                        foreach (DataOverrideItem item1 in group1.Items)
                        {
                            foreach (DataOverrideItem item2 in group2.Items)
                            {
                                if (item1.SettingsNameId == item2.SettingsNameId &&
                                    item1.DocId != item2.DocId)
                                {
                                    if (overrideSB.Length == 0)
                                    {
                                        overrideSB.Append("Overlapping data overrides: ");
                                    }
                                    SettingsName sn = gs.data.Get<SettingsNameSettings>(null).Get(item1.SettingsNameId);
                                    overrideSB.Append("In " + group1.Name + " and " + group2.Name + " " + sn.Name +
                                        " have the same settings name at the same priority.");
                                    foundBadData = true;
                                }
                            }
                        }
                    }
                }

                if (overrideSB.Length > 0)
                {
                    await ShowMessageBox(overrideSB.ToString());
                }


                if (foundBadData)
                {
                    return;
                }

                StringBuilder saveList = new StringBuilder();

                foreach (ITopLevelSettings settings in gs.data.AllSettings())
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        await ShowMessageBox("Setting object blank Id of type " + settings.GetType().Name);
                        foundBadData = true;
                        return;
                    }

                    settings.SetInternalIds();
                }


                List<IGameSettings> settingsToSave = new List<IGameSettings>();

                gs.LookedAtObjects = gs.LookedAtObjects.Distinct().ToList();

                foreach (object obj in gs.LookedAtObjects) // Grouping, not saving
                {
                    if (obj is IGameSettings settings)
                    {
                        if (settings is IIdName idn)
                        {
                            if (idn.IdKey == 0)
                            {
                                continue;
                            }
                        }

                        settingsToSave.Add(settings);
                    }
                }

                List<IGrouping<Type, IGameSettings>> groupingList =
                    settingsToSave.GroupBy(x => x.GetType()).ToList();

                groupingList = groupingList.OrderBy(x => x.Key.Name).ToList();

                foreach (IGrouping<Type, IGameSettings> group in groupingList)
                {
                    saveList.Append(group.Key.Name + ": ");

                    List<IGameSettings> orderedList = group.OrderBy(x => x.Id).ToList();

                    for (int i = 0; i < orderedList.Count; i++)
                    {
                        saveList.Append(orderedList[i].Id + (i < orderedList.Count - 1 ? ", " : "\n"));
                    }
                }

                EContentDialogResult result = await ShowMessageBox(saveList.ToString(), "Save This Data?", true);

                if (result != EContentDialogResult.Primary)
                {
                    return;
                }

                // Set Save time to before the data is saved so it's older than anything that's saved now.
                VersionSettings versionSettings = gs.data.Get<VersionSettings>(null);
                DateTime updateTime = DateTime.UtcNow;

                if (!gs.LookedAtObjects.Contains(versionSettings))
                {
                    gs.LookedAtObjects.Add(versionSettings);
                }

                gs.LookedAtObjects = gs.LookedAtObjects.Distinct().ToList();

                List<BaseGameSettings> settingsList = new List<BaseGameSettings>();
                foreach (object obj in gs.LookedAtObjects) // Saving
                {
                    if (obj is BaseGameSettings baseGameSetting)
                    {

                        if (obj is IIdName idn)
                        {
                            if (idn.IdKey == 0)
                            {
                                continue;
                            }
                        }

                        settingsList.Add(baseGameSetting);
                        baseGameSetting.SaveTime = updateTime;
                    }
                    else
                    {
                        Console.WriteLine("Not a game setting: " + obj.GetType().Name);
                    }
                }


                ISmallPopup popup = await ShowBlockingDialog("Saving Game Data");
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    _taskService.ForgetTask(SaveSettingsList(settingsList, popup, copyData), false);

                });

            }

            else if (action == "Users")
            {
                _taskService.ForgetTask(_dataService.SaveEditorUserData(gs), true);
            }
        }

        private async Task SaveSettingsList(List<BaseGameSettings> settingsList, ISmallPopup popup, bool copyData)
        {
            await SaveSettingsListInternal(settingsList, copyData);

            this.DispatcherQueue.TryEnqueue(() => { popup.StartClose(); });
        }

        private async Task SaveSettingsListInternal(List<BaseGameSettings> settingsList, bool copyData)
        {

            if (EnvNames.IsProdEnv(_serverConfig.Env))
            {
                copyData = false;
            }

            List<IGameSettings> gameSettingsList = settingsList.Cast<IGameSettings>().ToList();
            while (settingsList.Count > 0)
            {
                List<Task> saveTasks = new List<Task>();
                for (int i = 0; i < 20; i++)
                {
                    if (settingsList.Count == 0)
                    {
                        break;
                    }
                    saveTasks.Add(_repoService.Save(settingsList.Last()));
                    settingsList.RemoveAt(settingsList.Count - 1);
                }

                await Task.WhenAll(saveTasks);
                await Task.Delay(100);

                if (settingsList.Count == 0)
                {
                    break;
                }
            }

            if (copyData)
            {
                _dataService.WriteGameDataListToGit(gameSettingsList);
                _dataService.WriteGameDataToClient(gameSettingsList);
            }
            _gs.LookedAtObjects = new List<object>();

        }


        public String ShowStack()
        {
            string txt = "";

            for (int i = 0; i < ViewStack.Count; i++)
            {
                DataView dv = ViewStack[i] as DataView;
                if (dv == null)
                {
                    continue;
                }

                object obj = dv.GetObject();
                object par = dv.GetParent();
                if (obj == null)
                {
                    continue;
                }

                Type type = obj.GetType();

                object idObj = _reflectionService.GetObjectValue(obj, GameDataConstants.IdKey);

                if (idObj == null)
                {
                    idObj = "";
                }

                string idStr = idObj.ToString();

                object nameObj = _reflectionService.GetObjectValue(obj, "Name");

                if (!String.IsNullOrEmpty(txt))
                {
                    txt += " >>> ";
                }

                string mname = _reflectionService.GetMemberName(par, obj);
                if (string.IsNullOrEmpty(mname))
                {
                    mname = type.Name;
                }

                if (mname.IndexOf("BackingField") >= 0)
                {
                    mname = "List";
                }

                txt += mname;
                if (!String.IsNullOrEmpty(idStr))
                {
                    txt += " [#" + idStr + "] ";
                    if (nameObj != null && !string.IsNullOrEmpty(nameObj.ToString()))
                    {
                        txt += nameObj.ToString() + " ";
                    }
                }

            }

            return txt;
        }

    }
}




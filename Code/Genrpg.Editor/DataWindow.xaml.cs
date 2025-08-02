using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Reflection;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Interfaces;
using Genrpg.Editor.Utils;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Settings.Settings;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Versions.Settings;
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

        private IRepositoryService _repoService = null;
        private IServerConfig _config = null;
        private ITaskService _taskService = null;

        private EditorGameState gs = null;
        public IList<UserControlBase> ViewStack = null;
        private Object obj = null;
        public String action = "";
        private WindowBase _parentForm;


        int width = 2400;
        int height = 1350;

        public int Width => width;
        public int Height => height;

        private CanvasBase _canvas = new CanvasBase();
        public void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public void Remove(object cont) { _canvas.Remove(cont); }
        public bool Contains(object cont) { return _canvas.Contains(cont); }


        public DataWindow(EditorGameState gsIn, Object objIn, WindowBase parentFormIn, String actionIn)
        {
            Content = _canvas;
            _parentForm = parentFormIn;
            gs = gsIn;
            gs.loc.Resolve(this);
            action = actionIn;
            ViewStack = new List<UserControlBase>();
            obj = objIn;
            if (obj == null)
            {
                return;
            }

            UIHelper.SetWindowRect(this, 50, 50, width, height);

            AddView(action);

        }
        public void AddView(String action)
        {
            UserControlFactory ucf = new UserControlFactory();
            UserControlBase view = null;
            if (action == "Users")
            {
                view = new FindUserView(gs, this);
            }
            else if (action == "Data")
            {
                view = ucf.Create(gs, this, obj, null, null, null);
            }
            else if (action == "Map")
            {
                view = ucf.Create(gs, this, obj, null, null, null);
            }
            else if (action == "CopyToTest")
            {
                view = new CopyDataView(gs, this);
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

        public async Task SaveData()
        {

            String env = _config.DefaultEnv;

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

                IGameDataService gds = gs.loc.Get<IGameDataService>();

                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                bool foundBadData = false;

                List<IGrouping<Type, ITopLevelSettings>> groups = gs.data.AllSettings().GroupBy(x => x.GetType()).ToList();


                List<ITopLevelSettings> allSettings = gs.data.AllSettings();
                foreach (ITopLevelSettings settings in allSettings)
                {
                    if (string.IsNullOrEmpty(settings.Id))
                    {
                        await UIHelper.ShowMessageBox(this, "Setting object blank Id of type " + settings.GetType().Name);
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
                                    await UIHelper.ShowMessageBox(this, sb.ToString());
                                    foundBadData = true;
                                }
                            }

                        }
                    }

                    List<IGrouping<string, ITopLevelSettings>> nameGroups = items.GroupBy(x => x.Id).ToList();

                    if (items.Count != nameGroups.Count)
                    {
                        await UIHelper.ShowMessageBox(this, "Setting " + group.Key.Name + " has duplicate DocId");
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
                    await UIHelper.ShowMessageBox(this, overrideSB.ToString());
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
                        await UIHelper.ShowMessageBox(this, "Setting object blank Id of type " + settings.GetType().Name);
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

                ContentDialogResultBase result = await UIHelper.ShowMessageBox(this, saveList.ToString(), "Save This Data?", true);

                if (result != ContentDialogResultBase.Primary)
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
                }


                this.DispatcherQueue.TryEnqueue(() =>
                {
                    SmallPopup popup = UIHelper.ShowBlockingDialog(this, "Saving Game Data");

                    _taskService.ForgetTask(SaveSettingsList(settingsList, popup), false);

                });

            }

            else if (action == "Users")
            {
                Task.Run(() => EditorPlayerUtils.SaveEditorUserData(gs, _repoService).GetAwaiter().GetResult()).GetAwaiter().GetResult();
            }
        }

        private async Task SaveSettingsList(List<BaseGameSettings> settingsList, SmallPopup popup)
        {
            await SaveSettingsListInternal(settingsList);
            this.DispatcherQueue.TryEnqueue(() => { popup.StartClose(); });
        }

        private async Task SaveSettingsListInternal(List<BaseGameSettings> settingsList)
        {
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
            gs.LookedAtObjects = new List<object>();

        }


        public String ShowStack()
        {
            string txt = "";

            IEditorReflectionService reflectionService = gs.loc.Get<IEditorReflectionService>();

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

                object idObj = reflectionService.GetObjectValue(obj, GameDataConstants.IdKey);

                if (idObj == null)
                {
                    idObj = "";
                }

                string idStr = idObj.ToString();

                object nameObj = reflectionService.GetObjectValue(obj, "Name");

                if (!String.IsNullOrEmpty(txt))
                {
                    txt += " >>> ";
                }

                string mname = reflectionService.GetMemberName(par, obj);
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

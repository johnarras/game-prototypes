using Genrpg.DataUtils.Constants;
using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Interfaces;
using Genrpg.DataUtils.Services.EditorData;
using Genrpg.DataUtils.Utils;
using Genrpg.Editor.UI;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Tasks.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Genrpg.Editor
{
    public partial class FindUserView : UserControlBase, IUICanvas
    {
        private IFullRepositoryService _repoService = null;
        private IEditorDataService _dataService = null;
        private ITaskService _taskService = null;
        private EditorGameState _gs = null;
        private DataWindow _win = null;
        private TextBoxBase _queryInput = null;
        private ComboBoxBase _queryType = null;
        private CommunityToolkit.WinUI.UI.Controls.DataGrid Grid = null;


        public FindUserView(EditorGameState gsIn, DataWindow winIn)
        {
            Content = _canvas;
            _gs = gsIn;
            _gs.loc.Resolve(this);
            _win = winIn;
            if (_win != null)
            {
                Width = _win.Width;
                Height = _win.Height;
                _win.AddChildView(this);
                _win.ViewStack.Add(this);
            }
            ShowComponents();
        }

        public void ShowComponents()
        {
            int x = 50;
            int width = 150;
            int height = 30;
            int ypos = 100;
            PropertyInfo[] props = typeof(Account).GetProperties();

            List<string> wordlist = new List<string>();

            foreach (PropertyInfo prop in props)
            {
                if (prop.Name.IndexOf("Password") >= 0)
                {
                    continue;
                }

                wordlist.Add(prop.Name);
            }

            string[] words = wordlist.ToArray();


            _queryType = UIHelper.CreateComboBoxBase(this, "SaerchType", width, height, x, 20);

            _queryType.ItemsSource = words;
            if (words != null && words.Length > 0)
            {
                _queryType.SelectedItem = "Id";
            }


            _queryInput = UIHelper.CreateTextBoxBase(this, "Query", null, width, height, 0, 60, null);

            int currX = x;
            UIHelper.CreateButton(this, EButtonTypes.TopBar, "SearchButton", "Search", width, height, x, ypos, OnClickSearch); currX += width + 5;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "ClearButton", "Clear", width, height, x + width + 5, ypos, OnClickClear); currX += width + 5;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "DetailsButton", "Details", width, height, currX, ypos, OnClickDetails); currX += (width + 5) * 3;

            UIHelper.CreateButton(this, EButtonTypes.TopBar, "DeleteButton", "Delete", width, height, currX, ypos, OnClickDelete);

            Grid = UIHelper.CreateDataGridView(this, "UserGrid", _win.Width - 17, _win.Height - 180, 0, 140);
        }
        private void OnClickClear(object sender, object e)
        {

            Grid.ItemsSource = null;
        }

        private void OnClickDetails(object sender, object e)
        {
            object row = Grid.SelectedItem;

            Account acct = row as Account;
            if (acct == null)
            {
                return;
            }

            if (_gs == null || _gs.loc == null || _win == null)
            {
                return;
            }

            ISmallPopup form = _win.ShowBlockingDialog("Loading user data").Result;
            _taskService.ForgetTask(_dataService.LoadEditorUserData(_gs, acct.Id), false);
            form.StartClose();
            if (_gs.EditorUser.GameAccount == null)
            {
                _win.ShowMessageBox("User Not Found").Wait();
                return;
            }

            UserControlFactory ucf = new UserControlFactory();
            UserControlBase view = ucf.Create(_gs, _win, _gs.EditorUser, null, null, null);


        }

        private void OnClickDelete(object sender, object e)
        {
            System.Collections.IList rows = Grid.SelectedItems;
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            Account acct = rows[0] as Account;
            if (acct == null)
            {
                return;
            }

            ISmallPopup form = _win.ShowBlockingDialog("Loading user data").Result;
            _taskService.ForgetTask(_dataService.LoadEditorUserData(_gs, acct.Id) , false);

            form.StartClose();
            form = _win.ShowBlockingDialog("Deleting user data").Result;

            // We don't delete the account here.
            _taskService.ForgetTask(_dataService.DeleteEditorUserData(_gs), false);
           
            form.StartClose();

        }

        private void OnClickSearch(object sender, object e)
        {
            string val = _queryInput.Text;
            Object item = _queryType.SelectedItem;
            _ = Task.Run(() => OnClickSearchAsync(val, item));
        }

        private async Task OnClickSearchAsync(string val, object item)
        {
            if (item == null)
            {
                return;
            }

            string key = item.ToString();
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(val))
            {
                return;
            }

            List<Account> list = await _repoService.Search<Account>(x => x.Id == val);


            DispatcherQueue.TryEnqueue(() =>
            {
                Grid.ItemsSource = list;
            });
            await Task.CompletedTask;
        }

    }
}




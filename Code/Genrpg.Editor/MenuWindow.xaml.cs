using CommunityToolkit.WinUI;
using Genrpg.DataUtils.Constants;
using Genrpg.DataUtils.Entities.Copying;
using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Interfaces;
using Genrpg.DataUtils.Services.EditorData;
using Genrpg.DataUtils.Utils;
using Genrpg.Editor.UI;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{

    public class ButtonClickAction
    {
        public string ButtonName { get; set; }
        public Action<object,object> ClickAction { get; set; }

        public ButtonClickAction(string buttonName, Action<object,object> clickAction)
        {
            ButtonName = buttonName;
            ClickAction = clickAction;
        }
    }

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MenuWindow : WindowBase, IUICanvas
    {
        const int _topPadding = 50;

        private string _prefix;
        private string _env;



        public MenuWindow()
        {
            Content = _canvas;
            _prefix = Game.Prefix;
            int buttonCount = 0;

            _env = MainMenuWindow.CurrentEnv;

            bool isProd = EnvNames.IsProdEnv(_env);

            UIHelper.CreateLabel(this, ELabelTypes.Default, _prefix + "Label", _env + " Editor", getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;


            List<ButtonClickAction> actionWords = new List<ButtonClickAction>();
            actionWords.Add(new ButtonClickAction("Data", OnClickDataButton));
            if (!isProd)
            {
                actionWords.Add(new ButtonClickAction("Importer", ClickImporter));
                actionWords.Add(new ButtonClickAction("CopyToGit", ClickCopyFromDatabaseToGit));
                actionWords.Add(new ButtonClickAction("CopyToClient", ClickCopyFromDatabaseToClient));
                actionWords.Add(new ButtonClickAction("SerializeSetup", ClickSerializeSetup));
            }

            actionWords.Add(new ButtonClickAction("CopyToServer", ClickRefreshServerGameData));
            actionWords.Add(new ButtonClickAction("Users", OnClickDataButton));
            actionWords.Add(new ButtonClickAction("Maps", OnClickMaps));
            actionWords.Add(new ButtonClickAction("CopyToDb", ClickCopyFromGitToDatabase));


            actionWords.Add(new ButtonClickAction("GeminiApi", OnClickGeminiApi));
            int column = 0;

            if (string.IsNullOrEmpty(_env))
            {
                return;
            }

            for (int a = 0; a < actionWords.Count; a++)
            {
                ButtonClickAction action = actionWords[a];

                if (string.IsNullOrEmpty(action.ButtonName) ||
                    action.ClickAction == null)
                {
                    continue;
                }


                UIHelper.CreateButton(this,
                    EButtonTypes.Default,
                    action.ButtonName,
                    action.ButtonName,
                    getButtonWidth(),
                    getButtonHeight(),
                    getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                    getTotalHeight(buttonCount),
                    action.ClickAction);
                buttonCount++;
            }

            UIHelper.SetWindowRect(this, 100, 100,
                 2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2),
            getTotalHeight(buttonCount) + getTopBottomPadding() + _topPadding);

        }

        private int getButtonWidth() { return 150; }

        private int getButtonHeight() { return 40; }

        private int getLeftRightPadding() { return 20; }

        private int getTopBottomPadding() { return 10; }

        private int getButtonGap() { return 8; }

        private int getTotalHeight(int numButtons)
        {
            return (getButtonHeight() + getButtonGap()) * numButtons + getTopBottomPadding();
        }

        private void OnClickDataButton(object sender, object e)
        {
            Task.Run(() => OnClickButtonAsync(sender, null));
        }

        private void OnClickMaps(object sender, object e)
        {
            
            Task.Run(() => OnClickButtonAsync(sender, null));
        }

        private void ClickCopyFromDatabaseToGit(object sender, object e)
        {
            Task.Run(() => OnClickButtonAsync(sender, CopyGameDataFromDatabaseToGitAsync));
        }

        private void ClickCopyFromGitToDatabase(object sender, object e)
        {
            Task.Run(() => OnClickButtonAsync(sender, CopyGameDataFromGitToDatabaseAsync));
        }
        
        private void ClickCopyFromDatabaseToClient(object sender, object e)
        {
            Task.Run(() => OnClickButtonAsync(sender, CopyGameDataFromDatabaseToClientAsync));
        }

        private void ClickRefreshServerGameData(object sender, object e)
        {
            Task.Run(() => OnClickButtonAsync(sender, RefreshServerDataAsync)); ;
        }

        private void ClickSerializeSetup(object sender, object e)
        {
            Task.Run(() => OnClickButtonAsync(sender, SerializeSetupAsync)); 
        }

        private void OnClickGeminiApi(object sender, object e)
        {
            GeminiApiWindow window = new GeminiApiWindow(_env);
            window.Activate();
        }
        private async Task SerializeSetupAsync (EditorGameState gs, IEditorDataService gameDataService, CancellationToken token)
        {
            gameDataService.InitSerialization();
        }

        private void ClickImporter(object sender, object e)
        {
            ImportWindow importer = new ImportWindow(_env);
            importer.Activate();
        }

        private async Task OnClickButtonAsync(object sender, OnEditorClickAction afterAction = null)
        {

            DispatcherQueue.TryEnqueue(async () =>
            {
                ButtonBase button = sender as ButtonBase;
                ISmallPopup form = await ShowBlockingDialog(StrUtils.SplitOnCapitalLetters(button?.Name ?? "Loading Data"));
                EditorDataSetup eds = new EditorDataSetup();
                await eds.SetupGameState(this, _env, true, button.Name, afterAction);
                form.StartClose();
            });
        }

        private async Task CopyGameDataFromDatabaseToGitAsync(EditorGameState gs, IEditorDataService gameDataService, CancellationToken token)
        {
            gameDataService.WriteAllGameDataToGit(gs);
        }

        private async Task CopyGameDataFromGitToDatabaseAsync(EditorGameState gs, IEditorDataService gameDataService, CancellationToken token)
        {
            try
            {
                await gameDataService.CopyFromGitToDb(gs, token);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        private async Task RefreshServerDataAsync(EditorGameState gs, IEditorDataService gameDataService, CancellationToken token)
        {
            gs.loc.Get<ICloudCommsService>().SendPubSubMessage(new UpdateGameDataAdminMessage());
        }


        private async Task CopyGameDataFromDatabaseToClientAsync(EditorGameState gs, IEditorDataService gameDataService, CancellationToken token)
        {
            try
            {
                DateTime saveTime = DateTime.UtcNow;
                gameDataService.WriteGameDataToClient(gs.data.AllSettings().Cast<IGameSettings>().ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}





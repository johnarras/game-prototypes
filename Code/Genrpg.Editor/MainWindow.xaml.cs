using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Copying;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Interfaces;
using Genrpg.Editor.Utils;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : WindowBase, IUICanvas
    {
        const int _topPadding = 50;

        private EditorGameState _gs = null;
        private string _prefix;

        private IGameDataService _gameDataService = null;
        private IRepositoryService _repoService = null;
        private ICloudCommsService _cloudCommsService = null;
        private ITextSerializer _serializer = null;

        private CanvasBase _canvas = new CanvasBase();
        public void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public void Remove(object cont) { _canvas.Remove(cont); }
        public bool Contains(object cont) { return _canvas.Contains(cont); }


        public MainWindow()
        {
            Content = _canvas;
            _prefix = Game.Prefix;
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _prefix + "Label", _prefix, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "dev" };
            string[] actionWords = "Data Importer CopyToGit CopyToClient CopyToServers MessageSetup Users  CopyToTest Maps CopyToDB TestAccountSetup".Split(' ');
            int column = 0;
            for (int e = 0; e < envWords.Length; e++)
            {
                string env = envWords[e];

                if (env != EnvNames.Dev)
                {
                    continue;
                }
                if (env != EnvNames.Test)
                {
                    for (int a = 0; a < actionWords.Length; a++)
                    {
                        string action = actionWords[a];

                        if (string.IsNullOrEmpty(action))
                        {
                            continue;
                        }

                        if (env == EnvNames.Prod && action == "Data")
                        {
                            continue;
                        }

                        if (action == "Maps")
                        {
                            if (_prefix == Game.Prefix)
                            {

                                UIHelper.CreateButton(this,
                                    EButtonTypes.Default,
                                    env + " " + action,
                                    _prefix + " " + env + " " + action,
                                    getButtonWidth(),
                                    getButtonHeight(),
                                    getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                                    getTotalHeight(buttonCount),
                                    OnClickMaps);
                                buttonCount++;
                            }
                            continue;
                        }



                        UIHelper.CreateButton(this,
                            EButtonTypes.Default,
                            env + " " + action,
                            env + " " + action,
                            getButtonWidth(),
                            getButtonHeight(),
                            getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                            getTotalHeight(buttonCount),
                            OnClickButton);
                        buttonCount++;
                    }
                }

            }


            UIHelper.CreateButton(this,
                EButtonTypes.Default,
                "TestSharedRandom",
                "Test Shared Random",
                getButtonWidth(),
                getButtonHeight(),
                getLeftRightPadding() + column * (getButtonWidth() + column * getButtonGap()),
                getTotalHeight(buttonCount),
                OnClickSharedRandom);
            buttonCount++;

            UIHelper.SetWindowRect(this, 100, 100,
                 2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2),
            getTotalHeight(buttonCount) + getTopBottomPadding() + _topPadding);

        }

        private void OnClickSharedRandom(object sender, object e)
        {
            TestSharedRandomAsync().Wait();
        }

        private async Task TestSharedRandomAsync()
        {
            int numTasks = 100;
            int iterationCount = 100000;

            DateTime startTime = DateTime.UtcNow;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(TestRegRandom(iterationCount));
            }

            await Task.WhenAll(tasks);

            double randSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

            startTime = DateTime.UtcNow;

            tasks.Clear();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(TestSharedRandom(iterationCount));
            }

            await Task.WhenAll(tasks);

            double sharedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;

            Trace.WriteLine("RandSeconds: " + randSeconds + " " + totalRandVal + " SharedSeconds: " + sharedSeconds + " " + totalSharedVal);

        }

        private long totalRandVal = 0;
        private long totalSharedVal = 0;
        private async Task TestRegRandom(int iterations)
        {
            Random rand = new Random();
            long val = 0;
            for (int i = 0; i < iterations; i++)
            {
                val += rand.Next() + rand.Next() + rand.Next() + rand.Next() + rand.Next() +
                   rand.Next() + rand.Next() + rand.Next() + rand.Next() + rand.Next();
            }

            totalRandVal += val;
            await Task.CompletedTask;
        }

        private async Task TestSharedRandom(int iterations)
        {
            long val = 0;
            for (int i = 0; i < iterations; i++)
            {
                val += Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() +
                    Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next() + Random.Shared.Next();
            }

            totalSharedVal += val;
            await Task.CompletedTask;
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

        private void OnClickMaps(object sender, object e)
        {

            _ = Task.Run(() => OnClickMapsAsync(sender, e));
        }

        private async Task OnClickMapsAsync(object sender, object e)
        {
            ButtonBase but = sender as ButtonBase;
            if (but == null)
            {
                return;
            }

            String txt = but.Content.ToString();
            if (String.IsNullOrEmpty(txt))
            {
                return;
            }

            string[] words = txt.Split(' ');
            if (words.Length < 3)
            {
                return;
            }

            if (string.IsNullOrEmpty(_prefix))
            {
                return;
            }

            String env = words[1];
            String action = words[2];

            _gs = await EditorGameDataUtils.SetupFromConfig(this, env, true);

        }

        private void OnClickButton(object sender, object e)
        {
            ButtonBase but = sender as ButtonBase;
            if (but == null)
            {
                return;
            }

            String txt = but.Name;
            if (String.IsNullOrEmpty(txt))
            {
                return;
            }
            string[] words = txt.Split(' ');
            if (words.Length < 2)
            {
                return;
            }

            if (string.IsNullOrEmpty(_prefix))
            {
                return;
            }

            String env = words[0];
            String action = words[1];

            if (action == "CopyToTest")
            {
                CopyDataFromEnvToEnv(EnvNames.Dev, EnvNames.Test);
                return;
            }

            if (action == "CopyToGit")
            {
                CopyGameDataFromDatabaseToGit(EnvNames.Dev);
                return;
            }
            if (action == "CopyToDB")
            {
                CopyGameDataFromGitToDatabase(EnvNames.Dev);
                return;
            }
            if (action == "CopyToClient")
            {
                CopyGameDataFromDatabaseToClient(EnvNames.Dev);
                return;
            }
            else if (action == "CopyToServers")
            {

                _ = Task.Run(() => RefreshServerDataAsync(env));
                return;
            }

            if (action == "MessageSetup")
            {
                EditorGameDataUtils.InitMessages();
                return;
            }

            if (action == "Importer")
            {
                ImportWindow importer = new ImportWindow();
                importer.Activate();
                return;

            }

            Task.Run(() => OnClickButtonAsync(action, env, null));
        }


        private async Task OnClickButtonAsync(string action, string env, Action<EditorGameState> afterAction = null)
        {
            _gs = await EditorGameDataUtils.SetupForEditing(this, action, env, afterAction);
        }

        private void CopyDataFromEnvToEnv(string fromEnv, string toEnv)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying data from " + fromEnv + " to " + toEnv);
            _ = Task.Run(() => CopyDataFromEnvToEnvAsync(fromEnv, toEnv, form));
        }

        private async Task CopyDataFromEnvToEnvAsync(string fromEnv, string toEnv, SmallPopup form)
        {

            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, fromEnv, EditorGameState.CTS.Token);
            await EditorGameDataUtils.SaveFullGameData(this, dataCopy, toEnv, true, EditorGameState.CTS.Token);

            form.StartClose();
        }

        private void CopyGameDataFromDatabaseToGit(string env)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying to Git");
            _ = Task.Run(() => CopyGameDataFromDatabaseToGitAsync(env, form, EditorGameState.CTS.Token));
        }

        private async Task CopyGameDataFromDatabaseToGitAsync(string env, SmallPopup form, CancellationToken token)
        {
            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, env, token);

            EditorGameDataUtils.WriteGameDataToDisk(dataCopy, _serializer);

            form.StartClose();
        }

        private void CopyGameDataFromGitToDatabase(string env)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying to Mongo");
            _ = Task.Run(() => CopyGameDataFromGitToDatabaseAsync(form, env, EditorGameState.CTS.Token));
        }

        private async Task CopyGameDataFromGitToDatabaseAsync(SmallPopup form, string env, CancellationToken token)
        {
            FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadDataFromDisk(form, _serializer, token);
            await EditorGameDataUtils.SaveFullGameData(form, dataCopy, env, true, token);

            form.StartClose();
        }

        private async Task RefreshServerDataAsync(string env)
        {
            _gs = await EditorGameDataUtils.SetupFromConfig(this, env, true);

            _cloudCommsService.SendPubSubMessage(new UpdateGameDataAdminMessage());
        }


        private void CopyGameDataFromDatabaseToClient(string env)
        {
            SmallPopup form = UIHelper.ShowBlockingDialog(this, "Copying to Client");
            _ = Task.Run(() => CopyGameDataFromDatabaseToClientAsync(env, form, EditorGameState.CTS.Token));
        }

        private async Task CopyGameDataFromDatabaseToClientAsync(string env, SmallPopup form, CancellationToken token)
        {

            try
            {
                FullGameDataCopy dataCopy = await EditorGameDataUtils.LoadFullGameData(this, env, token);

                Dictionary<Type, IGameSettingsMapper> mapperDict = _gameDataService.GetAllMappers();

                List<ITopLevelSettings> clientSettings = new List<ITopLevelSettings>();
                foreach (IGameSettings gameSettings in dataCopy.Data)
                {
                    if (gameSettings is ITopLevelSettings topLevelSettings)
                    {
                        if (mapperDict.TryGetValue(topLevelSettings.GetType(), out IGameSettingsMapper mapper))
                        {
                            if (mapper.SendToClient())
                            {
                                clientSettings.Add(mapper.MapToDto(topLevelSettings, true));
                            }
                        }
                    }
                }

                EditorGameDataUtils.WriteGameDataToClient(clientSettings, _serializer);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            form.StartClose();
        }
    }
}


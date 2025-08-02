using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers;
using Genrpg.Editor.UI;
using Genrpg.Editor.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.HelperClasses;
using System;
using System.Threading.Tasks;
using Genrpg.Editor.UI.Interfaces;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImportWindow : WindowBase, IUICanvas
    {
        const int _topPadding = 50;

        private string _prefix;

        private SetupDictionaryContainer<EImportTypes, IDataImporter> _importers = new();

        private CanvasBase _canvas = new CanvasBase();
        public void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public void Remove(object cont) { _canvas.Remove(cont); }
        public bool Contains(object cont) { return _canvas.Contains(cont); }


        public ImportWindow()
        {
            Content = _canvas;
            _prefix = Game.Prefix;
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _prefix + "Label", _prefix, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "Import" };

            string[] actionWords = Enum.GetNames(typeof(EImportTypes));

            int column = 0;
            for (int e = 0; e < envWords.Length; e++)
            {
                string env = envWords[e];

                for (int a = 0; a < actionWords.Length; a++)
                {
                    string action = actionWords[a];

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

            UIHelper.SetWindowRect(this, 100, 100,
                 2 * getLeftRightPadding() + 1 * (getButtonWidth() + getButtonGap() * 2) + 500,
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

            Action<EditorGameState> afterAction = null;


            string[] actionWords = Enum.GetNames(typeof(EImportTypes));

            EImportTypes importType = Enum.Parse<EImportTypes>(action);

            afterAction = (gs) => { ImportData(gs, importType); };
            action = "Data";


            Task.Run(() => OnClickButtonAsync(action, env, afterAction));
        }


        private async Task OnClickButtonAsync(string action, string env, Action<EditorGameState> afterAction = null)
        {
            await EditorGameDataUtils.SetupForEditing(this, action, env, afterAction);
        }

        private void ImportData(EditorGameState gs, EImportTypes importType)
        {

            _ = Task.Run(() => ImportDataAsync(gs, importType));
        }


        private async Task ImportDataAsync(EditorGameState gs, EImportTypes importType)
        {
            gs.loc.Resolve(_importers);

            try
            {
                if (_importers.TryGetValue(importType, out IDataImporter importer))
                {
                    await importer.ImportData(this, gs);
                }
            }
            catch (Exception ex)
            {
                await UIHelper.ShowMessageBox(this, ex.Message, "Exception", false);
            }
        }
    }
}
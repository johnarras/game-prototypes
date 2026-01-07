using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers;
using Genrpg.Editor.UI;
using Genrpg.Editor.UI.Interfaces;
using Genrpg.Editor.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private List<IDataImporter> _importers = null;

        private CanvasBase _canvas = new CanvasBase();
        public void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public void Remove(object cont) { _canvas.Remove(cont); }
        public bool Contains(object cont) { return _canvas.Contains(cont); }


        private ComboBoxBase _comboBox = null;
        class ImporterName
        {
            public IDataImporter Importer { get; set; }
            public string Name { get; set; }
        }

        public ImportWindow()
        {
            Content = _canvas;
            _prefix = Game.Prefix;
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _prefix + "Label", _prefix, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "Import" };

            List<Type> importTypes = ReflectionUtils.GetTypesImplementing(typeof(IDataImporter));

            _importers = new List<IDataImporter>();
            foreach (Type importType in importTypes)
            {
                _importers.Add((IDataImporter)EntityUtils.DefaultConstructor(importType));
            }

            _importers = _importers.OrderBy(x => x.HelperKey.Name).ToList();

            List<ImporterName> importNames = new List<ImporterName>();

            importNames.Add(new ImporterName() { Importer = null, Name = "None" });
            foreach (IDataImporter imp in _importers)
            {
                importNames.Add(new ImporterName()
                {
                    Importer = imp,
                    Name = "Import " + imp.HelperKey.Name,
                });
            }

            int startx = 100;
            int starty = 100;
            int cx = startx;
            int cy = starty;

            _comboBox = UIHelper.CreateComboBoxBase(this, "ImportDropdown", getButtonWidth(), getButtonHeight(), cx, cy);

            cy += getButtonHeight() + getButtonGap();

            _comboBox.ItemsSource = importNames;
            _comboBox.DisplayMemberPath = nameof(ImporterName.Name);
            _comboBox.SelectedValuePath = nameof(ImporterName.Importer);



            UIHelper.CreateButton(this,
            EButtonTypes.Default,
            "ImportButton", "Import Data", getButtonWidth(), getButtonHeight(), cx, cy, OnClickButton);

            cy += getButtonHeight() + getButtonGap();

            UIHelper.SetWindowRect(this, startx, starty, startx + getButtonWidth() + 500, cy + getButtonHeight() + 200);
        }
        private int getButtonWidth() { return 250; }

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

            if (_comboBox == null)
            {
                return;
            }

            ImporterName selectedImporter = _comboBox.SelectedItem as ImporterName;

            if (selectedImporter == null || selectedImporter.Importer == null)
            {
                return;
            }

            Action<EditorGameState> afterAction = null;


            IDataImporter importer = selectedImporter.Importer;
            string action = "";
            if (importer != null)
            {
                afterAction = (gs) => { ImportData(gs, importer); };
                action = "Data";

                Task.Run(() => OnClickButtonAsync(action, "Import", afterAction));
            }
        }


        private async Task OnClickButtonAsync(string action, string env, Action<EditorGameState> afterAction = null)
        {
            await EditorGameDataUtils.SetupForEditing(this, action, env, afterAction);
        }

        private void ImportData(EditorGameState gs, IDataImporter importer)
        {

            _ = Task.Run(() => ImportDataAsync(gs, importer));
        }


        private async Task ImportDataAsync(EditorGameState gs, IDataImporter importer)
        {
            gs.loc.Resolve(importer);

            try
            {
                await importer.ImportData(this, gs);
            }
            catch (Exception ex)
            {
                await UIHelper.ShowMessageBox(this, ex.Message, "Exception", false);
            }
        }
    }
}



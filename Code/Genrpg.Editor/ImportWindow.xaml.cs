using Genrpg.Editor.UI;
using OxDb.DataUtils.Constants;
using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers;
using OxDb.DataUtils.Interfaces;
using OxDb.DataUtils.Utils;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
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

        private string _productName;
        private string _env;
        private List<IInjectable> _initialServices = null;

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

        public ImportWindow(List<IInjectable> initialServices, string productName, string env)
        {
            _productName = productName;
            _env = env;
            _initialServices = initialServices;
            Content = _canvas;
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _productName + "Label", _productName, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "Import" };

            ReflectionService reflectionService = new ReflectionService();
            List<Type> importTypes = reflectionService.GetTypesImplementing(typeof(IDataImporter));

            _importers = new List<IDataImporter>();
            foreach (Type importType in importTypes)
            {
                _importers.Add((IDataImporter)reflectionService.DefaultConstructor(importType));
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


            IDataImporter importer = selectedImporter.Importer;
            string action = "Import";
            if (importer != null)
            {
                Task.Run(() => OnClickButtonAsync(action, _env, importer));
            }
        }




        private async Task OnClickButtonAsync(string action, string env, IDataImporter importer)
        {

            DispatcherQueue.TryEnqueue(async () =>
            {
                ISmallPopup form = await ShowBlockingDialog(StrUtils.SplitOnCapitalLetters("Importing " + importer.GetType().Name.Replace("Importer", "")));
                EditorDataSetup eds = new EditorDataSetup();
                await eds.SetupEditorServer(this, _initialServices, _env, true, "Data", async (server, gs, gds, token) => { await ImportDataAsync(gs, importer); });
                form.StartClose();
            });
        }

        private async Task ImportDataAsync(EditorGameState gs, IDataImporter importer)
        {
            gs.loc.Resolve(importer);

            try
            {
                await importer.ImportData(gs);
            }
            catch (Exception ex)
            {
                await ShowMessageBox(ex.Message, "Exception", false);
            }
        }
    }
}

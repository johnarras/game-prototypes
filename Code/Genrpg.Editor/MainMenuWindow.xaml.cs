using Genrpg.Editor.UI;
using OxDb.DataUtils.Constants;
using OxDb.DataUtils.Interfaces;
using OxDb.ServerCore.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Names.Entities;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenuWindow : WindowBase, IUICanvas
    {

        private string _currentEnv = null;

        private string _productName = null;

        private List<IInjectable> _initialServices = null;

        private CanvasBase _canvas = new CanvasBase();
        public void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public void Remove(object cont) { _canvas.Remove(cont); }
        public bool Contains(object cont) { return _canvas.Contains(cont); }


        private TextBoxBase _suffixInput = null;

        private ComboBoxBase _comboBox = null;


        public MainMenuWindow(List<IInjectable> initialServices)
        {
            _initialServices = initialServices;
            Content = _canvas;
            _productName = ServerConfigUtils.GetHardcodedConfigValue(AppConfigKeys.ProductName);
            int buttonCount = 0;


            UIHelper.CreateLabel(this, ELabelTypes.Default, _productName + "Label", _productName, getButtonWidth(), getButtonHeight(),
                getLeftRightPadding(), getTopBottomPadding(), 20);
            buttonCount++;

            string[] envWords = { "Env" };

            List<KeyValue> envNames = ConstantUtils.GetStringConstants(typeof(EnvNames));

            envNames = envNames.Where(x => x.Key.ToLower() != EnvNames.Local.ToLower()).ToList();

            int startx = 100;
            int starty = 100;
            int cx = startx;
            int cy = starty;

            _comboBox = UIHelper.CreateComboBoxBase(this, "EnvDropdown", getButtonWidth(), getButtonHeight(), cx, cy);

            cy += getButtonHeight() + getButtonGap();

            _comboBox.ItemsSource = envNames;
            _comboBox.DisplayMemberPath = nameof(KeyValue.Key);
            _comboBox.SelectedValuePath = nameof(KeyValue.Key);
            _comboBox.SelectedIndex = 0;

            UIHelper.CreateLabel(this, ELabelTypes.Default, "DevSuffixLabel", "Dev Suffix:", getButtonWidth(), getButtonHeight(), cx, cy);
            _suffixInput = UIHelper.CreateTextBoxBase(_canvas, "DevSuffix", "", getButtonWidth(), getButtonHeight(), cx + getButtonWidth(), cy, null);
            cy += getButtonHeight() + getButtonGap();

            UIHelper.CreateButton(this,
            EButtonTypes.Default,
            "StartButton", "Manage Data", getButtonWidth(), getButtonHeight(), cx, cy, OnClickButton);

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


            KeyValue selectedItem = _comboBox.SelectedItem as KeyValue;

            string envName = selectedItem?.Val ?? null;

            if (string.IsNullOrEmpty(envName))
            {
                return;
            }

            string suffix = _suffixInput.Text;

            string fullEnv = envName;

            if (!string.IsNullOrEmpty(suffix))
            {
                if (suffix.IndexOf("-") != 0)
                {
                    fullEnv += "-" + suffix;
                }
                else
                {
                    fullEnv += suffix;
                }
            }

            _currentEnv = fullEnv;

            MenuWindow menuWindow = new MenuWindow(_initialServices, _productName, _currentEnv);
            menuWindow.Activate();

            Close();
        }
    }
}



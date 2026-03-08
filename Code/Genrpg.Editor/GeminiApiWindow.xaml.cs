using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Interfaces;
using Genrpg.DataUtils.Utils;
using Genrpg.Shared.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeminiApiWindow : WindowBase
    {
        public GeminiApiWindow(string env)
        {

            InitializeComponent();
            SendButton.Click += OnClickButton;
            DispatcherQueue.TryEnqueue(async () =>
            {
                ISmallPopup form = await ShowBlockingDialog(StrUtils.SplitOnCapitalLetters("Gemini Api"));
                EditorDataSetup eds = new EditorDataSetup();
                _gs = await eds.SetupGameState(this, env, true, "Image", null);
                form.StartClose();
            });
        }

        private EditorGameState _gs = null;
        private void OnClickButton(object sender, object e)
        {
            if (_gs == null)
            {
                ShowOutputText("Data Not Ready!");
                return;
            }

            ShowOutputText("Clicked Button!");
        }

        private void ShowOutputText(string txt)
        {
            DispatcherQueue.TryEnqueue(() => { StatusText.Text = "Status: " + txt; });
        }
    }
}

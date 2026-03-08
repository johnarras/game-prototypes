using Genrpg.DataUtils.Constants;
using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Interfaces;
using Genrpg.Editor.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class WindowBase : Window, IWindowBase
    {
        public void ShowDataWindow(EditorGameState gs, object data, string action)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                DataWindow win = new DataWindow(gs, gs.EditorGameData, this, action);

                win.Activate();
            });
        }

        public async Task<EContentDialogResult> ShowMessageBox(string content, string title = null, bool showCancelButton = false)
        {
            MessageBoxWaiter waiter = new MessageBoxWaiter();

            DispatcherQueue.TryEnqueue(() =>
            {
                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "Ok",
                    SecondaryButtonText = (showCancelButton ? "Cancel" : null),
                };

                noWifiDialog.XamlRoot = Content.XamlRoot;

                waiter.Operation = noWifiDialog.ShowAsync();
                waiter.DidSetOperation = true;
            });

            while (!waiter.DidSetOperation ||
               waiter.Operation.Status == AsyncStatus.Started)
            {
                await Task.Delay(100);
            }

            int val = (int)(waiter.Operation.GetResults());
            waiter.Result = (EContentDialogResult)val;
            return waiter.Result;

        }


        public async Task<ISmallPopup> ShowBlockingDialog(string text, double width = 0, double height = 0)
        {
            SmallPopup smallPopup = null;

            DispatcherQueue.TryEnqueue(() =>
            {
                SmallPopup tempPopup = new SmallPopup(text, (int)width, (int)height);
                tempPopup.Activate();
                smallPopup = tempPopup;
            });

            while (smallPopup == null)
            {
                await Task.Delay(100);
            }

            return smallPopup;
        }

        protected CanvasBase _canvas = new CanvasBase();
        public virtual void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public virtual void Remove(object cont) { _canvas.Remove(cont); }
        public virtual bool Contains(object cont) { return _canvas.Contains(cont); }
    }
}


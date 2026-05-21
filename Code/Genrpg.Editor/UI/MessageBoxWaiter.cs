using Microsoft.UI.Xaml.Controls;
using OxDb.DataUtils.Constants;
using Windows.Foundation;

namespace Genrpg.Editor.UI
{
    public class MessageBoxWaiter
    {

        public IAsyncOperation<ContentDialogResult> Operation { get; set; } = null;
        public bool DidSetOperation { get; set; } = false;
        public EContentDialogResult Result { get; set; } = EContentDialogResult.None;
    }
}



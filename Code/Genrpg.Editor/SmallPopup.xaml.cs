using Genrpg.DataUtils.Constants;
using Genrpg.DataUtils.Interfaces;
using Genrpg.Editor.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SmallPopup : WindowBase, ISmallPopup
    {
        public SmallPopup(string text, int width = 0, int height = 0)
        {

            if (width == 0)
            {
                width = 400;
            }

            if (height == 0)
            {
                height = 200;
            }

            Content = _canvas;

            UIHelper.SetWindowRect(this, 200, 200, width, height);

            int border = 50;

            UIHelper.CreateLabel(this, ELabelTypes.Default, "DialogText", text, width - 2*border, height-2*border, border, border,36);
        }

        public void StartClose()
        {
            DispatcherQueue.TryEnqueue(() => Close());
        }
    }
}




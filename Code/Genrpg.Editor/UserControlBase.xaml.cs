using Genrpg.DataUtils.Interfaces;
using Genrpg.Editor.UI;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Genrpg.Editor
{
    public partial class UserControlBase : UserControl, IUICanvas
    {
        public UserControlBase()
        {
            this.InitializeComponent();
        }

        protected CanvasBase _canvas = new CanvasBase();
        public void Add(object elem, double x, double y) { _canvas.Add(elem, x, y); }
        public void Remove(object cont) { _canvas.Remove(cont); }
        public bool Contains(object cont) { return _canvas.Contains(cont); }
    }
}




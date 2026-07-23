using System.Threading;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    [UxmlElement]
    public partial class GSlider : Slider
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle?.Token ?? CancellationToken.None;
        public GSlider() => _lifecycle = new UILifecycleHelper(this);
    }
}

using System.Threading;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.Overrides
{
    [UxmlElement]
    public partial class GToggle : Toggle
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle?.Token ?? CancellationToken.None;
        public GToggle() => _lifecycle = new UILifecycleHelper(this);
    }
}

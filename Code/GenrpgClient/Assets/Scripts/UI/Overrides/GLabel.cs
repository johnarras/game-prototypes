using System.Threading;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    [UxmlElement]
    public partial class GLabel : Label
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle?.Token ?? CancellationToken.None;
        public GLabel() => _lifecycle = new UILifecycleHelper(this);
    }
}

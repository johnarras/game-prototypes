using System.Threading;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    [UxmlElement]
    public partial class GTextField : TextField
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle?.Token ?? CancellationToken.None;
        public GTextField() => _lifecycle = new UILifecycleHelper(this);
    }
}

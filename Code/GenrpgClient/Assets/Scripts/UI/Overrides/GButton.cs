using System.Threading;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    [UxmlElement]
    public partial class GButton : Button
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle.Token;
        public GButton() => _lifecycle = new UILifecycleHelper(this);
    }
}
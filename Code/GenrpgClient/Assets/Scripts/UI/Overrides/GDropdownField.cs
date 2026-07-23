using System.Threading;
using UnityEngine.UIElements;

namespace OxDb.Client.UI.Overrides
{
    [UxmlElement]
    public partial class GDropdownField : DropdownField
    {
        private UILifecycleHelper _lifecycle;
        public CancellationToken DestroyToken => _lifecycle?.Token ?? CancellationToken.None;
        public GDropdownField() => _lifecycle = new UILifecycleHelper(this);
    }
}

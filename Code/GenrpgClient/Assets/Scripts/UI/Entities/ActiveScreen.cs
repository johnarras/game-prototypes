using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;

namespace Assets.Scripts.UI.Entities
{
    public class ActiveScreen
    {
        public IScreen Screen;
        public ScreenLayers LayerId;
        public long ScreenId;
        public object Data;
        public object LayerObject;
    }
}

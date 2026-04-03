using Assets.Scripts.UI.Interfaces;

namespace Assets.Scripts.UI.Entities
{
    public class ActiveScreen
    {
        public IScreen Screen;
        public long ScreenLayerId;
        public long ScreenId;
        public object Data;
        public object LayerObject;
    }
}



using System.Collections.Generic;

namespace Assets.Scripts.ClientEvents.UI
{
    public class CloseAllScreens
    {
        public List<long> KeepOpenScreens { get; set; } = new List<long>();

        public CloseAllScreens()
        {

        }

        public CloseAllScreens(List<long> keepOpenScreens)
        {
            KeepOpenScreens = keepOpenScreens;
        }
    }
}



using Genrpg.Shared.Client.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.ClientEvents.UI
{
    public class CloseAllScreens : IClientEvent
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



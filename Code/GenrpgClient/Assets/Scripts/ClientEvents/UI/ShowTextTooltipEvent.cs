using OxDb.SharedCore.Client.Interfaces;
using UnityEngine;

namespace OxDb.Client.ClientEvents.UI
{
    public class ShowTextTooltipEvent : IClientEvent
    {
        public Vector3 Position;
        public string Text;
    }
}



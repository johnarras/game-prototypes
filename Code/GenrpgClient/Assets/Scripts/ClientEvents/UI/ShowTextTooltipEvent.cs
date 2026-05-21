using OxDb.SharedCore.Client.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ClientEvents.UI
{
    public class ShowTextTooltipEvent : IClientEvent
    {
        public Vector3 Position;
        public string Text;
    }
}




using OxDb.SharedCore.Client.Interfaces;
using UnityEngine.InputSystem;

namespace OxDb.Client.UI.ClientEvents
{
    public class ClickKey : IClientEvent
    {
        public Key Key { get; set; }
    }
}



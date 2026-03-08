
using Genrpg.Shared.Client.Interfaces;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.ClientEvents
{
    public class ClickKey : IClientEvent
    {
        public Key Key { get; set; }
    }
}



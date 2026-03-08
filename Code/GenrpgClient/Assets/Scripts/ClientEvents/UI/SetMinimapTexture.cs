using Genrpg.Shared.Client.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ClientEvents.UI
{
    public class SetMinimapTexture : IClientEvent
    {
        public Texture2D Texture { get; set; }
    }
}



using OxDb.SharedCore.Client.Interfaces;
using UnityEngine;

namespace OxDb.Client.ClientEvents.UI
{
    public class SetMinimapTexture : IClientEvent
    {
        public Texture2D Texture { get; set; }
    }
}



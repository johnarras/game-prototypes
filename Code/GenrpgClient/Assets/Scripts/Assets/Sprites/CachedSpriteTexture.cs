using System;
using UnityEngine;

namespace OxDb.Client.Assets.Entities
{
    public class CachedSpriteTexture
    {
        public string SpriteName;
        public Sprite CurrSprite;
        public int Count;
        public DateTime LastTimeUsed = DateTime.UtcNow;
    }

}



using System;
using UnityEngine;

namespace Assets.Scripts.Assets.Entities
{
    public class CachedSpriteTexture
    {
        public string SpriteName;
        public Sprite CurrSprite;
        public int Count;
        public DateTime LastTimeUsed = DateTime.UtcNow;
    }

}



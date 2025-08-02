using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

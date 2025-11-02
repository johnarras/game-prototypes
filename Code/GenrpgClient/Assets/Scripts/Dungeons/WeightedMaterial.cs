using System;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class WeightedMaterial
    {
        public int Weight;
        public Material Mat;

        public void Clear()
        {
            Mat = null;
        }
    }
}

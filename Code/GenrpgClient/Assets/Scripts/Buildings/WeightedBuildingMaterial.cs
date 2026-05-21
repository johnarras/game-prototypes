using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    [Serializable]
    public class WeightedBuildingMaterial
    {
        public double Weight;
        public Material Mat;
        public List<Color> ColorTargets = new List<Color>();
    }
}



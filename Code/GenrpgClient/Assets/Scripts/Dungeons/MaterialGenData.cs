using Assets.Scripts.ProcGen.Materials.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Dungeons
{

    [Serializable]
    public class WeightedMaterialGenType
    {
        public double Weight;

        public EMaterialGenTypes WallGenType;
    }


    public class MaterialGenData : BaseBehaviour
    {

        public Material MainMaterial;
        public List<WeightedMaterialGenType> GenTypes = new List<WeightedMaterialGenType>();

        public List<Color> ForegroundColors;
        public List<Color> BackgroundColors;

        public List<Color> AccentColors;




        public void Clear()
        {
        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }
    }
}



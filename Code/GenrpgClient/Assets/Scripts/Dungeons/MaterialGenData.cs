using Assets.Scripts.ProcGen.Materials.Constants;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Dungeons
{

    [Serializable]
    public class WeightedMaterialGenType : IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; }

        public EMaterialGenTypes WallGenType;
    }

    [Serializable]
    public class ColorSet : IWeightedItem
    {
        [field: SerializeField]
        public double Weight { get; set; } = 100;
        public Color Foreground;
        public Color Background;

        public List<Color> Accents = new List<Color>();
    }

    public class MaterialGenData : BaseBehaviour
    {

        public Material MainMaterial;
        public List<WeightedMaterialGenType> GenTypes = new List<WeightedMaterialGenType>();

        public List<ColorSet> ColorSets = new List<ColorSet>();


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



using OxDb.Client.ProcGen.Materials.Constants;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.ProcGen.Materials
{

    public class ScaledColor
    {
        public Color Color;
        public float EffectThreshold;
    }
    public class MaterialGenState
    {
        public int Width;
        public int Height;
        public IRandom Rand;

        public EMaterialGenTypes GenType { get; set; }
        public MaterialGenSettingsData Settings { get; set; }

        public Color ForegroundMain;
        public List<ScaledColor> ForegroundNoise = new List<ScaledColor>();

        public Color BackgroundMain;
        public List<ScaledColor> BackgroundNoise = new List<ScaledColor>();

        public int MaterialIndex = 0;

        public float CornerPerturbChance { get; set; }
        public float VerticalPerturbChance { get; set; }
        public float MaxCornerPerturbScale { get; set; }

        public float RoundCornerMinSize { get; set; }
        public float RoundCornerMaxSize { get; set; }

        public int BlockRowCount { get; set; }

        public float MaxDistanceToCrevice { get; set; }

        public float RoundCornerDistFreq { get; set; }
        public float RoundCornerDistAmp { get; set; }
        public float RoundCornerDistPers { get; set; }
        public bool UseLargeBlocks { get; set; }

        public float CurvedWallChance = 0.1f;

        public List<CornerPoint> CornerPoints { get; set; } = new List<CornerPoint>();

        public MaterialGenBlock Block { get; set; }


    }
}

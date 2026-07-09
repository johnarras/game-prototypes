
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{
    public class MaterialGenSettingsData : ScriptableObject
    {

        public int TextureSize = 1024;

        public Color SpecularColor = Color.black;
        public float SmoothnessScale = 0.5f;
        public float BumpScale = 10.0f;

        public int MinBrickRows = 7;

        public int MaxBrickRows = 11;

        public float BrickAspectRatioDelta = 0.3f;
        public float MinBrickAspectRatio = 1.2f;
        public float MaxBrickAspectRatio = 2.0f;

        public float RoundCornerSizeDelta = 0.3f;
        public float RoundCornerMinSizePercent = 0.01f;
        public float RoundCornerMaxSizePercent = 0.025f;


        public float MinColorNoiseFreq = 10f;
        public float MaxColorNoiseFreq = 20f;
        public float MinColorNoiseAmp = 0.05f;
        public float MaxColorNoiseAmp = 0.1f;
        public float MinColorNoisePers = 0.02f;
        public float MaxColorNoisePers = 0.8f;
        public float MaxColorNoiseDelta = 0.4f;
        public int MinColorNoiseCount = 2;
        public int MaxColorNoiseCount = 4;
        public int MaxGrayscaleColorNoise = 2;
        public float ColorNoiseGrayscaleChance = 0.3f;
        public int ColorNoiseOctaves = 5;
        public float MaxColorNoiseBumpScale = 0.4f;
        public float ColorPerPixelNoiseDelta = 0.03f;


        public float MinNoiseEffectThreshold = 0;
        public float MaxNoiseEffectThreshold = 1;

        public float CornerPerturbAtAllChance = 1.0f;
        public float MinCornerPerturbChance = 0.2f;
        public float MaxCornerPerturbChance = 0.4f;
        public float MaxCornerPerturbScale = 0.3f;

        public float MinVerticalPerturbChance = 0.3f;
        public float MaxVerticalPerturbChance = 0.9f;

        public float MinDistanceToCrevicePercent = 0.01f;
        public float MaxDistanceToCrevicePercent = 0.03f;
        public float NoCreviceSmoothingChance = 0.2f;

        public float MinRoundCornerDistFreq = 10f;
        public float MaxRoundCornerDistFreq = 20f;
        public float MinRoundCornerDistAmp = 0.05f;
        public float MaxRoundCornerDistAmp = 0.1f;
        public float MinRoundCornerDistPers = 0.02f;
        public float MaxRoundCornerDistPers = 0.8f;


        public float MakeGrayscaleColorChangeChance = 0.4f;
        public float MinGrayscaleShift = -0.2f;
        public float MaxGrayscaleShift = 1.5f;
        public float MaxBrightnessDelta = 0.3f;

        public float MaxBrightnessBumpScale = 0.7f;

        public float ChangeBlockColorChance = 0.4f;
        public float MakeVeryRoundChance = 0.1f;
        public float RemoveBlockChance = 0.05f;

        public float CurvedWallMinChance = 0.0f;
        public float CurvedWallMaxChance = 0.2f;


        public int CrackCount = 50;
        public float CrackQuantityDelta = 0.25f;
        public int MaxCracksPerBlock = 5;
        public float CrackChangeDirChance = 0.15f;
        public float CrackChangeDirAngleDelta = 30f;

        public float CrackBrightnessMaxDelta = 0.2f;


#if UNITY_EDITOR
        [MenuItem("Assets/Create/ScriptableObjects/WallGenSettings", false, 0)]
        public static void Create()
        {
            ScriptableObjectUtils.CreateBasicInstance<MaterialGenSettingsData>();
        }
#endif

    }
}

using Assets.Scripts.ProcGen.Materials.Constants;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{

    public class TextureBlock
    {
        public int Index { get; set; }
        public int CX { get; set; }
        public int CZ { get; set; }
        public int CrackCount { get; set; }
    }


    public class MaterialGenBlock
    {
        protected int ArraySize => BumpHeights.GetLength(0);


        public float[,] Brightness;
        public float[,] BumpHeights;
        public float[,] GrayScaleScaling;
        public int[,] BlockIndexes;
        public bool[,] DidCheckBlockIndex;
        public Color[,] Colors;

        public List<TextureBlock> Blocks { get; set; } = new List<TextureBlock>();


        public int GetNextBlockIndex()
        {
            return Blocks.Count + 1;
        }

        public MaterialGenBlock(int width, int height, Color startColor, float startBrightness, float startAlpha)
        {
            Brightness = new float[width, height];
            BumpHeights = new float[width, height];
            GrayScaleScaling = new float[width, height];
            Colors = new Color[width, height];
            BlockIndexes = new int[width, height];
            DidCheckBlockIndex = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    BumpHeights[x, z] = startAlpha;
                    Colors[x, z] = startColor;
                    Brightness[x, z] = startBrightness;
                    GrayScaleScaling[x, z] = 0;
                }
            }
        }

        public void AddFrontBumpHeight(int x, int z, float bumpDelta)
        {
            if (x < 0 || z < 0 || x >= ArraySize || z >= ArraySize)
            {
                return;
            }

            float currVal = BumpHeights[x, z];

            float newVal = currVal + bumpDelta;

            if (newVal <= MaterialGenConstants.MaxRecessedBumpHeight)
            {
                newVal = MaterialGenConstants.MaxRecessedBumpHeight;
            }

            BumpHeights[x, z] = newVal;
        }
    }
}

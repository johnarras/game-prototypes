using Assets.Scripts.ProcGen.Materials.Constants;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{
    public class MaterialGenBlock
    {
        protected int ArraySize => BumpHeights.GetLength(0);


        public float[,] Brightness;
        public float[,] BumpHeights;
        public Color[,] Colors;

        public MaterialGenBlock(int width, int height, Color startColor, float startBrightness, float startAlpha)
        {
            Brightness = new float[width, height];
            BumpHeights = new float[width, height];
            Colors = new Color[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    BumpHeights[x, y] = startAlpha;
                    Colors[x, y] = startColor;
                    Brightness[x, y] = startBrightness;
                }
            }
        }

        public void AddFrontBumpHeight(int x, int y, float bumpDelta)
        {
            if (x < 0 || y < 0 || x >= ArraySize || y >= ArraySize)
            {
                return;
            }

            float currVal = BumpHeights[x, y];

            float newVal = currVal + bumpDelta;

            if (newVal <= MaterialGenConstants.MaxRecessedBumpHeight)
            {
                newVal = MaterialGenConstants.MaxRecessedBumpHeight;
            }

            BumpHeights[x, y] = newVal;
        }
    }
}

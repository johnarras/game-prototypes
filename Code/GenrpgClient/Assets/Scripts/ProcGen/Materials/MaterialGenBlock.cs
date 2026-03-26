using Assets.Scripts.ProcGen.Materials.Constants;
using Genrpg.Shared.Inventory.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{
    public class MaterialGenBlock
    {
        protected int ArraySize => BumpHeights.GetLength(0);


        public float[,] Brightness;
        public float[,] BumpHeights;
        public Color[,] Colors;

        public MaterialGenBlock(int size, Color startColor, float startBrightness, float startAlpha)
        {
            Brightness = new float[size, size];
            BumpHeights = new float[size, size];
            Colors = new Color[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
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

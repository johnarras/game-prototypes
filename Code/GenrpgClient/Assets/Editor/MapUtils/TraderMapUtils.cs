using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Trader.Maps.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.Editor.MapUtils
{
    public static class TraderMapUtils
    {
        const int RDelta = 1;
        const int GDelta = 1;
        const int BDelta = 1;

        const float RDistMult = 1.0f;
        const float GDistMult = 1.5f;
        const float BDistMult = 1.0f;

        const int pixelGap = 32;
        const int clusterCount = 24;

        static readonly string worldMapFile = "TraderWorldMap.jpg";
        static readonly string clusteredFile = "ClusteredWorldMap.jpg";
        static readonly string colorIndexFile = "WorldMapColorIndexes.bytes";


        [MenuItem("Tools/GetClusteredWorldMapColors")]
        public static void SliceBigWorldMap()
        {

            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();


            IGameData gameData = gs.loc.Get<IGameData>();

            IReadOnlyList<IndexedColor> colorList = gameData.Get<IndexedColorSettings>(null).GetData();


            string folder = "Assets/FullAssets/Trader/Images/WorldMap/";

            Texture2D bigMapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + worldMapFile);

            NewtonsoftTextSerializer serializer = new NewtonsoftTextSerializer();


            Texture2D newTex = new Texture2D(bigMapTex.width, bigMapTex.height, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);


            byte[] newBytes = new byte[bigMapTex.width * bigMapTex.height];


            for (int x = 0; x < bigMapTex.width; x++)
            {
                for (int y = 0; y < bigMapTex.height; y++)
                {
                    Color c = bigMapTex.GetPixel(x, y);

                    int r = GetClampedByteValue(c.r, pixelGap, RDelta);
                    int g = GetClampedByteValue(c.g, pixelGap, GDelta);
                    int b = GetClampedByteValue(c.b, pixelGap, BDelta);


                    IndexedColor closestColor = null;
                    double clostestDistance = 1000000;
                    foreach (IndexedColor indexedColor in colorList)
                    {

                        double dr = Math.Abs(r - indexedColor.R) * RDistMult;
                        double dg = Math.Abs(g - indexedColor.G) * GDistMult;
                        double db = Math.Abs(b - indexedColor.B) * BDistMult;

                        double dist = Math.Sqrt(dr * dr + dg * dg + db * db);

                        if (dist < clostestDistance)
                        {
                            closestColor = indexedColor;
                            clostestDistance = dist;
                        }
                    }
                    newTex.SetPixel(x, y, new Color(closestColor.R / 255.0f, closestColor.G / 255.0f, closestColor.B / 255.0f));
                    newBytes[x + (newTex.height - 1 - y) * newTex.width] = (byte)closestColor.IdKey;
                }
            }
            File.WriteAllBytes(folder + clusteredFile, newTex.EncodeToJPG(100));
            File.WriteAllBytes(folder + colorIndexFile, newBytes);
        }

        private static int GetClampedByteValue(float input, int pixelGap, int deltaMult)
        {
            int val = (int)(input * 255);

            val -= val % pixelGap;
            val += (pixelGap / 2) * deltaMult;


            return MathUtil.Clamp(0, val, 255);

        }
    }
}

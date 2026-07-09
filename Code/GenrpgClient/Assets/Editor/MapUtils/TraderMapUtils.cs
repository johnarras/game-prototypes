using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Trader.Maps.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        const int pixelGap = 48;

        static readonly string worldMapFile = "TraderWorldMap.jpg";
        static readonly string clusteredFile = "ClusteredWorldMap.jpg";
        static readonly string colorIndexFile = "WorldMapColorIndexes.bytes";


        [MenuItem("Tools/GetClusteredWorldMapColors")]
        public static void SliceBigWorldMap()
        {

            IClientGameState gs = EditorGameDataUtils.GetEditorGameState(true);

            IGameData gameData = gs.loc.Get<IGameData>();

            List<IndexedColor> colorList = gameData.Get<IndexedColorSettings>(null).GetData().ToList();

            string folder = "Assets/FullAssets/Trader/Images/WorldMap/";

            Texture2D bigMapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + worldMapFile);

            NewtonsoftTextSerializer serializer = new NewtonsoftTextSerializer();


            Texture2D newTex = new Texture2D(bigMapTex.width, bigMapTex.height, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);


            byte[] newBytes = new byte[bigMapTex.width * bigMapTex.height];

            List<IdVal> _colorCounts = new List<IdVal>();

            for (int x = 0; x < bigMapTex.width; x++)
            {
                for (int z = 0; z < bigMapTex.height; z++)
                {
                    Color c = bigMapTex.GetPixel(x, z);

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
                    newTex.SetPixel(x, z, new Color(closestColor.R / 255.0f, closestColor.G / 255.0f, closestColor.B / 255.0f));
                    newBytes[x + z * newTex.width] = (byte)closestColor.IdKey;

                    IdVal currCount = _colorCounts.FirstOrDefault(x => x.Id == closestColor.IdKey);

                    if (currCount == null)
                    {
                        currCount = new IdVal() { Id = closestColor.IdKey };
                        _colorCounts.Add(currCount);
                    }


                    currCount.Val++;
                }
            }
            File.WriteAllBytes(folder + clusteredFile, newTex.EncodeToJPG(100));
            File.WriteAllBytes(folder + colorIndexFile, newBytes);

            StringBuilder sb = new StringBuilder();


            _colorCounts = _colorCounts.OrderByDescending(x => x.Val).Take(16).ToList();


            List<IndexedColor> finalColors = new List<IndexedColor>();

            foreach (IdVal idv in _colorCounts)
            {
                finalColors.Add(colorList.FirstOrDefault(x => x.IdKey == idv.Id));
            }

            finalColors = finalColors.OrderBy(x => x.R).ThenBy(x => x.G).ThenBy(x => x.B).ToList();

            for (int c = 0; c < finalColors.Count; c++)
            {
                finalColors[c].IdKey = c + 1;
            }

            sb.AppendLine("header indexedcolor,Idkey,R,G,B,TextureTypeId");

            foreach (IndexedColor ic in finalColors)
            {
                sb.AppendLine("indexedcolor," + ic.IdKey + "," + ic.R + "," + ic.G + "," + ic.B + ",1");
            }


            UnityEngine.Debug.Log(sb.ToString());
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

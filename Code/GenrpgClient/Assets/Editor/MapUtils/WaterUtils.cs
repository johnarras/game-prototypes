using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.Editor.MapUtils
{
    public static class WaterUtils
    {

        static readonly string worldMapFile = "TraderWorldMap.jpg";
        static readonly string bigMapFile = "BigWorldMap.jpg";
        [MenuItem("Tools/SetupWaterMask")]
        public static void CreateWaterMask()
        {
            string folder = "Assets/FullAssets/Trader/Images/WorldMap/";


            string waterMaskFile = "WaterMask.bytes";

            string waterMaskImage = "WaterMaskImage.jpg";

            Texture2D worldMapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + worldMapFile);

            byte[] outputBytes = new byte[worldMapTex.width * worldMapTex.height / 8];

            Texture2D maskImage = new Texture2D(worldMapTex.width, worldMapTex.height, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);

            for (int x = 0; x < worldMapTex.width; x++)
            {
                for (int y = 0; y < worldMapTex.height; y++)
                {
                    int ty = worldMapTex.height - y - 1;
                    Color c = worldMapTex.GetPixel(x, ty);

                    int index = x + y * worldMapTex.width;

                    int byteIndex = (index) / 8;

                    int bitOffset = index % 8;

                    if (c.b >= 0.1f && c.b <= 0.4f && c.b > (c.r + c.g))
                    {
                        outputBytes[byteIndex] |= (byte)(1 << bitOffset);
                        maskImage.SetPixel(x, ty, Color.black);
                        // nothing
                    }
                    else
                    {
                        maskImage.SetPixel(x, ty, Color.white);
                    }
                }
            }

            maskImage.Apply();
            File.WriteAllBytes(folder + waterMaskFile, outputBytes);
            File.WriteAllBytes(folder + waterMaskImage, maskImage.EncodeToJPG(100));
        }
        [MenuItem("Tools/SliceWorldImage")]
        public static void SliceBigWorldMap()
        {

            int xOffset = 7000;

            string folder = "Assets/FullAssets/Trader/Images/WorldMap/";


            Texture2D bigMapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + bigMapFile);

            int newWidth = 8192;
            int newHeight = 4096;

            int yOffset = bigMapTex.height - newHeight;

            Texture2D newTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    newTex.SetPixel(x, y, bigMapTex.GetPixel(x + xOffset, y + yOffset));
                }
            }

            newTex.Apply();

            File.WriteAllBytes(folder + worldMapFile, newTex.EncodeToJPG(100));

        }
    }
}

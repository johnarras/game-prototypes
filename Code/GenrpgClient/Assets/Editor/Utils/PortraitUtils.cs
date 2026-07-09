using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Portraits.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Utils
{
    public static class PortraitAssetUtils
    {
        [MenuItem("Tools/Create Portrait Textures")]
        public static void SetupPortraits()
        {
            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();
            IClientAppService clientAppService = gs.loc.Get<IClientAppService>();

            string parentPath = clientAppService.DataPath + "/FullAssets/Crawler/Images/";

            string startFolder = parentPath + "/PortraitsStart/";

            IRandom rand = new MyRandom(DateTime.UtcNow.Ticks);

            string endFolder = parentPath + "PortraitSprites/";

            string prefabFolder = parentPath + "/Portraits/";

            if (!Directory.Exists(startFolder))
            {
                Directory.CreateDirectory(startFolder);
            }

            if (!Directory.Exists(endFolder))
            {
                Directory.CreateDirectory(endFolder);
            }

            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
            }

            int portraitCount = 0;

            List<string> directories = Directory.GetDirectories(startFolder).ToList();

            directories.Add(startFolder);

            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory, "*.png");

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);

                    portraitCount++;

                    // Do not use number here.

                    string portraitsuffix = PortraitUtils.GetFileSuffixFromIndex(portraitCount);

                    string newFilename = "Portrait" + portraitsuffix + ".png";

                    string finalPath = Path.Combine(endFolder, newFilename);

                    File.Copy(file, finalPath, true);

                    File.Copy(file + ".meta", finalPath + ".meta", true);
                }
            }

            Debug.Log("PORTRAITCOUNT: " + portraitCount);
        }

        [MenuItem("Tools/Create Portrait Prefabs")]
        public static void SetupPrefabs()
        {
            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();
            IClientAppService clientAppService = gs.loc.Get<IClientAppService>();

            string assetPathSuffix = "/FullAssets/Crawler/Images";

            string parentPath = clientAppService.DataPath + assetPathSuffix;


            IRandom rand = new MyRandom(DateTime.UtcNow.Ticks);

            string spriteFolder = "Assets" + assetPathSuffix + "/PortraitSprites/";

            string prefabFolder = "Assets" + assetPathSuffix + "/Portraits/";

            string[] newFiles = Directory.GetFiles(spriteFolder, "*.png");

            foreach (string newFile in newFiles)
            {
                string fileName = Path.GetFileName(newFile);

                string spritePath = Path.Combine(spriteFolder, fileName);

                string prefabPath = Path.Combine(prefabFolder, fileName).Replace(".png", ".prefab");


                object obj = AssetDatabase.LoadAssetAtPath(spritePath, typeof(System.Object));

                Console.WriteLine("Obj: " + obj);

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (sprite == null)
                {
                    continue;
                }

                GameObject tempGo = new GameObject(Path.GetFileNameWithoutExtension(fileName));

                SpriteList sl = tempGo.AddComponent<SpriteList>();

                if (sl.Sprites == null)
                {
                    sl.Sprites = new List<Sprite>();
                }

                sl.Sprites.Add(sprite);

                PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);

                UnityEngine.Object.DestroyImmediate(tempGo);

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

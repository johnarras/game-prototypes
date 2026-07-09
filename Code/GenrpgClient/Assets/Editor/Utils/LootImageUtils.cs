using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Portraits.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Utils
{
    public static class LootAssetUtils
    {
        [MenuItem("Tools/Create Loot Images")]
        public static void SetupLootImages()
        {
            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();
            IClientAppService clientAppService = gs.loc.Get<IClientAppService>();

            string parentPath = clientAppService.DataPath + "/FullAssets/Crawler/Images/";

            string startFolder = parentPath + "RpgIcons/";

            IRandom rand = new MyRandom(DateTime.UtcNow.Ticks);

            string endFolder = parentPath + "FinalIcons/";

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

            float portraitChance = 1.0f;

            int portraitCount = 0;

            string[] directories = Directory.GetDirectories(startFolder);

            foreach (string directory in directories)
            {
                if (directory.ToLower().IndexOf("loot") < 0)
                {
                    continue;
                }
                string[] files = Directory.GetFiles(directory, "*.png", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    if (rand.NextDouble() > portraitChance)
                    {
                        continue;
                    }

                    string fileName = Path.GetFileName(file);

                    portraitCount++;

                    // Do not use number here.

                    string portraitsuffix = PortraitUtils.GetFileSuffixFromIndex(portraitCount);

                    string newFilename = "LootItem" + portraitsuffix + ".png";

                    string finalPath = Path.Combine(endFolder, newFilename);

                    File.Copy(file, finalPath, true);

                    File.Copy(file + ".meta", finalPath + ".meta", true);
                }
            }

            Debug.Log("PORTRAITCOUNT: " + portraitCount);
        }

        [MenuItem("Tools/Create Loot Images")]
        public static void SetupPrefabs()
        {
            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();
            IClientAppService clientAppService = gs.loc.Get<IClientAppService>();

            string assetPathSuffix = "/FullAssets/Crawler/Images/";

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

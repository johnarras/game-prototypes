using Assets.Scripts.Assets.TMP;
using Assets.Scripts.Awaitables;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Utils
{

    public class FilenameTextPair
    {
        public string FullFilePath { get; set; }
        public string PrintedText { get; set; }
    }

    public class TextureGenList
    {
        public List<FilenameTextPair> FilenameTextPairs { get; set; } = new List<FilenameTextPair>();
        public int TextureSize { get; set; } = 256;
        public Color BGColor { get; set; } = Color.clear;
    }

    public static class TextureGenUtils
    {


        [MenuItem("Tools/Create Entity Icons")]
        public static void GenerateEntityIcons()
        {
            GenerateItemIcons<EntitySettings, EntityType>("/FullAssets/Atlas");
        }

        public static void GenerateItemIcons<TParent,TChild> (string mainFolderPath) where TParent : ParentSettings<TChild>
            where TChild : ChildSettings, IIndexedGameItem, new()
        {

            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();

            IGameData gameData = gs.loc.Get<IGameData>();


            TParent settings = gameData.Get<TParent>(null);

            IReadOnlyList<TChild> children = settings.GetData();

            TextureGenList list = new TextureGenList()
            {
                BGColor = Color.clear,
                TextureSize = 256,

            };

            IClientAppService _appService = gs.loc.Get<IClientAppService>();

            string fullFolderPath = mainFolderPath + "/" + typeof(TChild).Name + "Icons";

            string diskFolderPath = _appService.DataPath + fullFolderPath;

            

            if (!Directory.Exists(diskFolderPath))
            {
                Directory.CreateDirectory(diskFolderPath);  
            }

            foreach (TChild child in children)
            {
                if (!string.IsNullOrEmpty(child.Name) && !string.IsNullOrEmpty(child.Icon))
                {
                    list.FilenameTextPairs.Add(new FilenameTextPair()
                    {
                        FullFilePath = fullFolderPath + "/" + child.Icon + ".png",
                        PrintedText = StrUtils.SplitOnCapitalLetters(child.Name).Replace(" ", "\n")
                    });


                }
            }

            GenerateTextures(gs, list);
        }


        [MenuItem("Tools/Update Monster Images")]
        public static void CreateMonsterNameTextures()
        {
            CreateNameTexturesFromFolder("/FullAssets/Crawler/Images/Monsters", Color.clear);
        }


        public static void CreateNameTexturesFromFolder(string assetFolderPath, Color bgColor, int textureSize = 256)
        {
            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();

            TextureGenList genList = new TextureGenList()
            {
                TextureSize = textureSize,
                BGColor = bgColor,
            };


            IClientAppService _appService = gs.loc.Get<IClientAppService>();


            string fullPath = _appService.DataPath + assetFolderPath;

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            string[] allFiles = Directory.GetFiles(fullPath);

            foreach (string startFullFilePath in allFiles)
            {

                string fullFilePath = startFullFilePath.Replace("\\", "/");

                string filename = Path.GetFileName(startFullFilePath);

                if (filename.LastIndexOf(".png") != filename.Length - 4)
                {
                    continue;
                }

                string innerName = filename.Replace(".png", "");

                string finalName = StrUtils.SplitOnCapitalLetters(StrUtils.GetAlphaChars(innerName));

                finalName = finalName.Replace(" ", "\n");

                if (!string.IsNullOrEmpty(finalName))
                {
                    genList.FilenameTextPairs.Add(new FilenameTextPair()
                    {
                        FullFilePath = fullFilePath,
                        PrintedText = finalName,
                    });
                }
            }

            GenerateTextures(gs, genList);
        }



        public static void GenerateTextures(IClientGameState gs, TextureGenList genList)
        {
            IAwaitableService _awaitableService = gs.loc.Get<IAwaitableService>();

            Debug.Log("List: " + genList);

           _awaitableService.ForgetAwaitable(GenerateTexturesAsync(gs, genList));
        }

        private static async Awaitable GenerateTexturesAsync(IClientGameState gs, TextureGenList list)
        {

            RenderTextMeshProToFile file = null;
            try
            {
                RenderTextMeshProToFile fileStart = Resources.Load<RenderTextMeshProToFile>("Prefabs/RenderTextMeshProToFile");


                file = GameObject.Instantiate(fileStart);

                int times = 0;
                foreach (FilenameTextPair pair in list.FilenameTextPairs)
                {
                    await file.RenderInputTextAsync(pair.PrintedText, pair.FullFilePath, list.BGColor, list.TextureSize);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            GameObject.DestroyImmediate(file.gameObject);
            AssetDatabase.SaveAssets();
        }
    }
}

using OxDb.Client.Crawler.Maps.Props;
using OxDb.Editor.Utils;
using OxdeadbeefGames.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.EntityEditors
{
    public static class CreatePropPrefabs
    {
        [MenuItem("Tools/Crawler/Create Prop Prefabs")]
        public static void CreateCrawlerPropPrefabs()
        {
            AssetPrefabArgs<MapProp> args = new AssetPrefabArgs<MapProp>()
            {
                StartFolder = "/FullAssets/Crawler/EnvironmentArt/TreePack/models",
                EndFolder = "/FullAssets/Crawler/NaturePrefabs/",
                Suffix = ".fbx",
                LocalRotation = new Vector3(0, 0, 0),

                LocalScale = Vector3.one,
            };


            AssetPrefabProcessor.ProcessAssetsToPrefabs<MapProp>(args);
        }


        [MenuItem("Tools/Images/Modify Image Values")]
        public static void ProcessNatureImages()
        {
            ImageBatchProcessor.ProcessFolder("Assets/FullAssets/Crawler/EnvironmentArt/TreePack/textures",
                0.0f, 0.0f, 0.1f);
        }
    }
}

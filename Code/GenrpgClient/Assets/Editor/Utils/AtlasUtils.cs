using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Utils
{
    public static class AtlasUtils
    {
        [MenuItem("Tools/Create TradeGood Icons")]
        public static void SetupTradeGoodIcons()
        {
            CreateEntityIcons(EntityTypes.TradeGood);
        }

        public static void CreateEntityIcons(long entityTypeId, int textureSize = 256, int iconQuantity = 1)
        {
            IClientGameState gs = EditorGameDataUtils.GetEditorGameState();

            IEntityService entityService = gs.loc.Get<IEntityService>();

            List<IIdName> entities = entityService.GetChildList(null, entityTypeId);

            if (entities.Count < 1)
            {
                Debug.LogError("EntityType " + entityTypeId + " does not exist.");
                return;
            }

            string entityTypeName = entities[0].GetType().Name;

            string folderName = entityTypeName + "Icons";

            IClientAppService clientAppService = gs.loc.Get<IClientAppService>();

            string iconFolder = clientAppService.DataPath + "/FullAssets/Atlas/" + folderName + "/";

            if (!Directory.Exists(iconFolder))
            {
                Directory.CreateDirectory(iconFolder);
            }


            foreach (IIdName iidname in entities)
            {

                IIndexedGameItem entity = iidname as IIndexedGameItem;

                if (entity == null || string.IsNullOrEmpty(entity.Icon))
                {
                    continue;
                }

                string fullAssetPath = Path.Combine(iconFolder, entity.Icon + ".png");

                AssetImporter startImporter = AssetImporter.GetAtPath(fullAssetPath) as AssetImporter;

                if (startImporter != null)
                {
                    continue;
                }

                Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false, true);
                for (int x = 0; x < textureSize; x++)
                {
                    for (int z = 0; z < textureSize; z++)
                    {
                        tex.SetPixel(x, z, Color.white);
                    }
                }

                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(fullAssetPath, bytes);

                AssetDatabase.ImportAsset(fullAssetPath, ImportAssetOptions.ForceUpdate);

                // 6. Set the import settings (TextureImporter) to make it a Sprite
                TextureImporter importer = AssetImporter.GetAtPath(fullAssetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    // Optionally set other settings like compression, pivot, etc.
                    importer.spriteImportMode = SpriteImportMode.Single;

                    // Re-apply the import settings
                    AssetDatabase.ImportAsset(fullAssetPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}



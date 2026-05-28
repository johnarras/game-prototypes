
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Assets.Editor.SpriteUtils
{
    public static class TextureToTerrainUtils
    {

        public static void CopyTexturesToTerrainTextures(string startFolder, string endFolder)
        {
            startFolder = SanitizePath(startFolder);    
            endFolder = SanitizePath(endFolder);
            if (!Directory.Exists(startFolder))
            {
                Debug.LogError($"Start folder does not exist: {startFolder}");
                return;
            }

            if (!Directory.Exists(endFolder))
            {
                Directory.CreateDirectory(endFolder);
            }

            string[] files = Directory.GetFiles(startFolder, "*Terrain.png");
            int processedCount = 0;

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath); // e.g., "ArcaneTerrain.png"
                string prefix = fileName.Replace("Terrain.png", ""); // e.g., "Arcane"

                string diffusePath = Path.Combine(endFolder, $"{prefix}_d.png");
                string normalPath = Path.Combine(endFolder, $"{prefix}_n.png");

                // 1. Copy to Diffuse (_d) without altering existing/default destination settings
                File.Copy(filePath, diffusePath, true);

                // 2. Copy to Normal (_n)
                File.Copy(filePath, normalPath, true);

                // Force Unity to notice the new files before we modify import settings
                AssetDatabase.ImportAsset(normalPath, ImportAssetOptions.ForceUpdate);

                // 3. Configure the normal map import settings
                TextureImporter textureImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.textureType = TextureImporterType.NormalMap;
                    textureImporter.convertToNormalmap = true; // Enables "Create from Grayscale"
                    textureImporter.heightmapScale = 0.25f;    // Adjust bumpiness depth if needed

                    // Reimport to apply the texture type changes
                    AssetDatabase.ImportAsset(normalPath, ImportAssetOptions.ForceUpdate);
                }

                processedCount++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"Successfully processed {processedCount} terrain texture pairs into {endFolder}!");
        }


        private static string SanitizePath(string path)
        {
            string cleanPath = path.Replace("\\", "/");
            if (!cleanPath.StartsWith("Assets"))
            {
                cleanPath = Path.Combine("Assets", cleanPath).Replace("\\", "/");
            }
            return cleanPath;
        }
    }
}


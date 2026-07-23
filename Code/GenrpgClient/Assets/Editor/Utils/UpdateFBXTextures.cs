using System.IO;
using UnityEditor;
using UnityEngine;

namespace OxDb.Editor.Utils
{
    public static class UpdateFBXTexturesAndMaterials
    {
        [MenuItem("Tools/Map TreePack Textures to Models")]
        public static void MapTreeTextures()
        {
            MapTexturesAndMaterials("Assets/FullAssets/Crawler/TreePack/");
        }

        public static void MapTexturesAndMaterials(string selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select the root folder containing the 'models' and 'textures' subfolders.", "OK");
                return;
            }

            string modelsFolder = Path.Combine(selectedPath, "models");
            string texturesFolder = Path.Combine(selectedPath, "textures");
            string materialsFolder = Path.Combine(selectedPath, "materials");

            if (!Directory.Exists(modelsFolder) || !Directory.Exists(texturesFolder))
            {
                EditorUtility.DisplayDialog("Error", "Selected folder must contain both 'models' and 'textures' subfolders.", "OK");
                return;
            }

            // Ensure a dedicated materials folder exists
            if (!Directory.Exists(materialsFolder))
            {
                Directory.CreateDirectory(materialsFolder);
                AssetDatabase.Refresh();
            }

            string[] fbxFiles = Directory.GetFiles(modelsFolder, "*.fbx", SearchOption.AllDirectories);
            int updatedCount = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < fbxFiles.Length; i++)
                {
                    string fbxFilePath = fbxFiles[i].Replace("\\", "/");
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fbxFilePath);

                    EditorUtility.DisplayProgressBar("Processing FBX Files", $"Updating materials for {fileNameWithoutExt}...", (float)i / fbxFiles.Length);

                    // 1. Find matching texture
                    string texturePath = Path.Combine(texturesFolder, fileNameWithoutExt + ".png").Replace("\\", "/");
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                    if (texture == null)
                    {
                        Debug.LogWarning($"[TextureMapper] No matching texture found at {texturePath} for model {fileNameWithoutExt}");
                        continue;
                    }

                    ModelImporter importer = AssetImporter.GetAtPath(fbxFilePath) as ModelImporter;
                    if (importer == null)
                    {
                        continue;
                    }

                    // Configure importer to import materials externalized or standard
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;

                    // 2. Load or Create an External Material
                    string materialPath = Path.Combine(materialsFolder, fileNameWithoutExt + ".mat").Replace("\\", "/");
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                    if (material == null)
                    {
                        // Use standard or URP default shader depending on your pipeline
                        Shader targetShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                        material = new Material(targetShader);
                        AssetDatabase.CreateAsset(material, materialPath);
                    }

                    // 3. Assign texture safely on the external asset
                    string texturePropertyName = material.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                    material.SetTexture(texturePropertyName, texture);
                    EditorUtility.SetDirty(material);

                    // 4. Remap internal slots to the external material asset
                    Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxFilePath);
                    foreach (Object subAsset in subAssets)
                    {
                        if (subAsset is Material embeddedMat)
                        {
                            AssetImporter.SourceAssetIdentifier sourceId = new AssetImporter.SourceAssetIdentifier(typeof(Material), embeddedMat.name);
                            importer.AddRemap(sourceId, material);
                        }
                    }

                    importer.SaveAndReimport();
                    updatedCount++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TextureMapper] Successfully created/assigned materials and textures for {updatedCount} FBX models.");
            EditorUtility.DisplayDialog("Success", $"Assigned materials and textures for {updatedCount} FBX models.", "OK");
        }
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.SpriteUtils
{
    public class SetBorderNearTransparentWindow : EditorWindow
    {
        private string folderName = "Assets/FullAssets/Crawler/Atlas/CrawlerMinimapAtlas";
        private Color newColor = Color.red;

        [MenuItem("Tools/Set Border Near Transparent")]
        public static void ShowWindow()
        {
            GetWindow<SetBorderNearTransparentWindow>("Color Replacer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Modify 'Wall' PNG Textures", EditorStyles.boldLabel);

            folderName = EditorGUILayout.TextField("Folder Path", folderName);
            newColor = EditorGUILayout.ColorField("New Color", newColor);

            if (GUILayout.Button("Process Textures"))
            {
                ProcessTextures();
            }
        }

        private void ProcessTextures()
        {
            if (!Directory.Exists(folderName))
            {
                Debug.LogError($"Directory not found: {folderName}");
                return;
            }

            // Find all PNG files in the folder and subfolders containing "Wall" in the name
            string[] fileEntries = Directory.GetFiles(folderName, "*Wall*.png", SearchOption.AllDirectories);
            int modifiedCount = 0;

            foreach (string filePath in fileEntries)
            {
                // Convert system path to project relative asset path for Unity's AssetImporter
                string assetPath = filePath.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, "");
                assetPath = assetPath.Replace('\\', '/'); // Ensure Unity-friendly slashes

                // Textures must be readable in RAM to manipulate pixels
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool wasReadable = importer.isReadable;
                TextureImporterCompression oldCompression = importer.textureCompression;

                if (!wasReadable || oldCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                // Load texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture != null)
                {
                    bool isDirty = false;


                    for (int x = 0; x < texture.width; x++)
                    {
                        for (int z = 0; z < texture.height; z++)
                        {
                            Color pix = texture.GetPixel(x, z);

                            if (pix.a > 0)
                            {
                                bool isNearTransparent = false;
                                for (int xx = x - 1; xx <= x + 1; xx++)
                                {
                                    if (isNearTransparent)
                                    {
                                        break;
                                    }
                                    if (xx < 0 || xx >= texture.width)
                                    {
                                        continue;
                                    }
                                    for (int zz = z - 1; zz <= z + 1; zz++)
                                    {
                                        if (zz < 0 || zz >= texture.height)
                                        {
                                            continue;
                                        }

                                        if (texture.GetPixel(xx, zz).a == 0)
                                        {
                                            isNearTransparent = true;
                                            break;
                                        }
                                    }
                                }

                                if (isNearTransparent)
                                {
                                    texture.SetPixel(x, z, newColor);
                                    isDirty = true;
                                }
                            }
                        }
                    }

                    if (isDirty)
                    {
                        texture.Apply();

                        // Encode and overwrite the original file
                        byte[] bytes = texture.EncodeToPNG();
                        File.WriteAllBytes(filePath, bytes);
                        modifiedCount++;
                    }
                }

                // Restore original import settings if we changed them
                if (!wasReadable || oldCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.isReadable = wasReadable;
                    importer.textureCompression = oldCompression;
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Successfully processed {modifiedCount} 'Wall' textures.");
        }
    }
}
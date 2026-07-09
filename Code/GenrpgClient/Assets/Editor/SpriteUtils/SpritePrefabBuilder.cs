using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetProcessing
{
    public class SpritePrefabBuilder
    {
        public static void ProcessSpritesToPrefabs(string spriteFolder, string outputFolder)
        {
            // Ensure paths are correctly formatted for AssetDatabase (Assets/...)
            string sanitizedSpriteFolder = SanitizePath(spriteFolder);
            string sanitizedOutputFolder = SanitizePath(outputFolder);

            if (!AssetDatabase.IsValidFolder(sanitizedSpriteFolder))
            {
                Debug.LogError($"Source sprite folder does not exist: {sanitizedSpriteFolder}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(sanitizedOutputFolder))
            {
                string[] folders = sanitizedOutputFolder.Split('/');
                string currentPath = folders[0]; // This will be "Assets"

                for (int i = 1; i < folders.Length; i++)
                {
                    string nextFolder = folders[i];
                    string testPath = $"{currentPath}/{nextFolder}";

                    if (!AssetDatabase.IsValidFolder(testPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, nextFolder);
                    }
                    currentPath = testPath;
                }
            }

            // Find all sprites in the source folder
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { sanitizedSpriteFolder });
            List<Sprite> allSprites = new List<Sprite>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    allSprites.Add(sprite);
                }
            }

            // Group sprites by their base name and sort them numerically
            // Regex matches the base name and captures the trailing numbers
            Regex nameRegex = new Regex(@"^(?<baseName>.*?)(?<number>\d+)$");
            Dictionary<string, List<Sprite>> groupedSprites = new Dictionary<string, List<Sprite>>();

            foreach (Sprite sprite in allSprites)
            {
                Match match = nameRegex.Match(sprite.name);
                if (match.Success)
                {
                    string baseName = match.Groups["baseName"].Value;
                    int number = int.Parse(match.Groups["number"].Value);

                    if (!groupedSprites.ContainsKey(baseName))
                    {
                        groupedSprites[baseName] = new List<Sprite>();
                    }
                    groupedSprites[baseName].Add(sprite);
                }
            }

            // Process each group to create or update prefabs
            foreach (KeyValuePair<string, List<Sprite>> pair in groupedSprites)
            {
                string baseName = pair.Key;

                if (baseName == "Bear")
                {
                    Debug.Log("Bear:");
                }

                // Sort by the trailing number in ascending order to cleanly handle gaps
                List<Sprite> sortedSprites = pair.Value
                    .OrderBy(s => int.Parse(nameRegex.Match(s.name).Groups["number"].Value))
                    .ToList();

                string prefabPath = Path.Combine(sanitizedOutputFolder, $"{baseName}.prefab").Replace("\\", "/");
                GameObject prefabRoot;
                bool isNewPrefab = false;

                // Load existing prefab or create a new GameObject
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (existingPrefab != null)
                {
                    prefabRoot = PrefabUtility.InstantiatePrefab(existingPrefab) as GameObject;
                }
                else
                {
                    prefabRoot = new GameObject(baseName);
                    isNewPrefab = true;
                }

                if (prefabRoot == null) continue;

                // Ensure SpriteList component exists and update its references
                SpriteList spriteListComp = prefabRoot.GetComponent<SpriteList>();
                if (spriteListComp == null)
                {
                    spriteListComp = prefabRoot.AddComponent<SpriteList>();
                }

                // Reset and repopulate the list
                spriteListComp.Sprites = new List<Sprite>(sortedSprites);

                // Save back to the asset database
                if (isNewPrefab)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, prefabPath, InteractionMode.AutomatedAction);
                }
                else
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }

                // Clean up the instantiated hierarchy instance
                Object.DestroyImmediate(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prefab sprite generation complete!");
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
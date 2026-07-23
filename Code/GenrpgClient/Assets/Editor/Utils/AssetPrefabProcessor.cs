using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OxdeadbeefGames.Editor.Tools
{

    public class AssetPrefabArgs<TMono> where TMono : MonoBehaviour
    {
        public string StartFolder;
        public string EndFolder;
        public string Suffix;
        public Action<TMono, UnityEngine.Object> OnConfigure;
        public Vector3 LocalPosition = Vector3.zero;
        public Vector3 LocalScale = Vector3.one;
        public Vector3 LocalRotation = Vector3.zero;

    }

    public static class AssetPrefabProcessor
    {
        /// <summary>
        /// Processes assets matching a suffix from a start folder, ensures they are packaged into a prefab inside an end folder,
        /// adds a specific MonoBehaviour component, and runs a custom configuration action.
        /// </summary>
        public static void ProcessAssetsToPrefabs<TMono>(
            AssetPrefabArgs<TMono> args) where TMono : MonoBehaviour
        {
            // 1. Sanitize and resolve the path strings safely
            string sanitizedStart = SanitizeRelativePath(args.StartFolder);
            string sanitizedEnd = SanitizeRelativePath(args.EndFolder);

            string systemStartPath = Path.Combine(Application.dataPath, sanitizedStart.Substring(7));
            string systemEndPath = Path.Combine(Application.dataPath, sanitizedEnd.Substring(7));

            if (!Directory.Exists(systemStartPath))
            {
                Debug.LogError($"[AssetPrefabProcessor] Start folder does not exist: {systemStartPath}");
                return;
            }

            if (!Directory.Exists(systemEndPath))
            {
                Directory.CreateDirectory(systemEndPath);
                AssetDatabase.Refresh();
            }

            // 2. Gather files matching the specified suffix
            string[] files = Directory.GetFiles(systemStartPath, $"*{args.Suffix}", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                Debug.LogWarning($"[AssetPrefabProcessor] No files matching suffix '{args.Suffix}' found in {sanitizedStart}");
                return;
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (string absoluteFilePath in files)
                {
                    // Convert system path back to a valid Unity Asset path
                    string relativeFilePath = ConvertToAssetPath(absoluteFilePath);
                    UnityEngine.Object startObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativeFilePath);

                    if (startObject == null)
                    {
                        continue;
                    }

                    string assetName = Path.GetFileNameWithoutExtension(absoluteFilePath);
                    string targetPrefabPath = $"{sanitizedEnd}/{assetName}.prefab";

                    GameObject rootInstance = null;
                    bool isNewPrefab = false;

                    // 3. Look for an existing prefab or prepare a fresh base root layout
                    GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPrefabPath);
                    if (existingPrefab != null)
                    {
                        rootInstance = PrefabUtility.InstantiatePrefab(existingPrefab) as GameObject;
                    }
                    else
                    {
                        rootInstance = new GameObject(assetName);
                        isNewPrefab = true;
                    }

                    if (rootInstance == null)
                    {
                        continue;
                    }

                    // 4. If the source asset is a GameObject, ensure it's nested correctly as a child
                    if (startObject is GameObject sourceGameObject)
                    {
                        // Prevent duplicate nesting if processing the same asset into an existing prefab again
                        Transform existingChild = rootInstance.transform.Find(sourceGameObject.name);
                        GameObject childInstance = null;

                        if (existingChild != null)
                        {
                            childInstance = existingChild.gameObject;
                        }
                        else
                        {
                            childInstance = PrefabUtility.InstantiatePrefab(sourceGameObject, rootInstance.transform) as GameObject;
                        }

                        if (childInstance != null)
                        {
                            childInstance.transform.localPosition = args.LocalPosition;
                            childInstance.transform.localRotation = Quaternion.Euler(args.LocalRotation);
                            childInstance.transform.localScale = args.LocalScale;
                        }
                    }

                    // 5. Ensure the requested MonoBehaviour component is attached
                    TMono targetComponent = rootInstance.GetComponent<TMono>();
                    if (targetComponent == null)
                    {
                        targetComponent = rootInstance.AddComponent<TMono>();
                    }

                    // 6. Execute the external configuration action hook
                    args.OnConfigure?.Invoke(targetComponent, startObject);

                    // 7. Save structural edits out back into the destination folder asset database footprint
                    if (isNewPrefab)
                    {
                        PrefabUtility.SaveAsPrefabAsset(rootInstance, targetPrefabPath);
                    }
                    else
                    {
                        PrefabUtility.SaveAsPrefabAssetAndConnect(rootInstance, targetPrefabPath, InteractionMode.AutomatedAction);
                    }

                    UnityEngine.Object.DestroyImmediate(rootInstance);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[AssetPrefabProcessor] Batch processing completed for {files.Length} items.");
        }

        /// <summary>
        /// Cleans paths, ensuring they begin with "Assets" and use forward slashes without trailing slashes.
        /// </summary>
        private static string SanitizeRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "Assets";
            }

            string clean = path.Replace('\\', '/').Trim('/');

            if (clean.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + clean.Substring(6);
            }

            if (string.Equals(clean, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            return $"Assets/{clean}";
        }

        private static string ConvertToAssetPath(string absolutePath)
        {
            string normalizedPath = absolutePath.Replace('\\', '/');
            int index = normalizedPath.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return normalizedPath.Substring(index + 1);
            }
            return normalizedPath;
        }
    }
}
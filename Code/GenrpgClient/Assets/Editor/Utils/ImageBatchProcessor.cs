using System.IO;
using UnityEditor;
using UnityEngine;

namespace OxDb.Editor.Utils
{
    public static class ImageBatchProcessor
    {




        /// <summary>
        /// Iterates over all images in a given project folder path and applies brightness, contrast, and saturation adjustments.
        /// </summary>
        /// <param name="folderPath">Project-relative path (e.g., "Assets/Textures/Environment") or full system path.</param>
        /// <param name="brightness">Range: -1.0 (black) to 1.0 (white), 0.0 is neutral.</param>
        /// <param name="contrast">Range: -1.0 (flat gray) to 1.0 (high contrast), 0.0 is neutral.</param>
        /// <param name="saturation">Range: -1.0 (grayscale) to 1.0 (2x saturation), 0.0 is neutral.</param>
        public static void ProcessFolder(string folderPath, float brightness, float contrast, float saturation)
        {
            string relativePath = ConvertToProjectRelativePath(folderPath);

            if (!AssetDatabase.IsValidFolder(relativePath))
            {
                Debug.LogError($"[ImageBatchProcessor] Invalid folder path provided: {folderPath}");
                return;
            }

            // Clamp parameter inputs strictly to [-1.0, 1.0]
            brightness = Mathf.Clamp(brightness, -1f, 1f);
            contrast = Mathf.Clamp(contrast, -1f, 1f);
            saturation = Mathf.Clamp(saturation, -1f, 1f);

            string systemPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            string[] searchPatterns = new string[] { "*.png", "*.jpg", "*.jpeg", "*.tga" };

            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (string pattern in searchPatterns)
                {
                    string[] filePaths = Directory.GetFiles(systemPath, pattern, SearchOption.AllDirectories);

                    foreach (string filePath in filePaths)
                    {
                        string assetPath = ConvertToProjectRelativePath(filePath);
                        ProcessSingleImage(assetPath, brightness, contrast, saturation);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[ImageBatchProcessor] Completed processing images in: {relativePath}");
        }

        private static void ProcessSingleImage(string assetPath, float brightness, float contrast, float saturation)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            // Ensure texture CPU read/write is enabled
            bool originallyReadable = importer.isReadable;
            if (!originallyReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (sourceTexture == null)
            {
                return;
            }

            // Perform core pixel manipulations
            Texture2D processedTexture = ModifyImagePixels(sourceTexture, brightness, contrast, saturation);

            // Encode and overwrite file on disk
            byte[] fileData = processedTexture.EncodeToPNG();
            string fullSystemPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            File.WriteAllBytes(fullSystemPath, fileData);

            Object.DestroyImmediate(processedTexture);

            // Re-import asset to register updated binary on disk
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Central method for core pixel adjustments using [-1.0, 1.0] parameters.
        /// </summary>
        public static Texture2D ModifyImagePixels(Texture2D sourceTexture, float brightness, float contrast, float saturation)
        {
            Color[] pixels = sourceTexture.GetPixels();

            // Calculate factors from normalized [-1, 1] inputs
            float contrastFactor = (contrast >= 0f) ? (1f + contrast * 3f) : (1f + contrast);
            float satFactor = saturation + 1f;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];

                // 1. Brightness (Additive shift)
                if (brightness != 0f)
                {
                    pixel.r += brightness;
                    pixel.g += brightness;
                    pixel.b += brightness;
                }

                // 2. Contrast (Scaled around 0.5 mid-gray point)
                if (contrast != 0f)
                {
                    pixel.r = (pixel.r - 0.5f) * contrastFactor + 0.5f;
                    pixel.g = (pixel.g - 0.5f) * contrastFactor + 0.5f;
                    pixel.b = (pixel.b - 0.5f) * contrastFactor + 0.5f;
                }

                // 3. Saturation (Linear interpolation toward luminance)
                if (saturation != 0f)
                {
                    float luminance = (pixel.r * 0.2126f) + (pixel.g * 0.7152f) + (pixel.b * 0.0722f);
                    pixel.r = Mathf.Lerp(luminance, pixel.r, satFactor);
                    pixel.g = Mathf.Lerp(luminance, pixel.g, satFactor);
                    pixel.b = Mathf.Lerp(luminance, pixel.b, satFactor);
                }

                pixel.r = Mathf.Clamp01(pixel.r);
                pixel.g = Mathf.Clamp01(pixel.g);
                pixel.b = Mathf.Clamp01(pixel.b);

                pixels[i] = pixel;
            }

            Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
            resultTexture.SetPixels(pixels);
            resultTexture.Apply();

            return resultTexture;
        }

        private static string ConvertToProjectRelativePath(string path)
        {
            string normalizedPath = path.Replace("\\", "/");
            if (normalizedPath.StartsWith(Application.dataPath))
            {
                return "Assets" + normalizedPath.Substring(Application.dataPath.Length);
            }
            return normalizedPath;
        }
    }
}
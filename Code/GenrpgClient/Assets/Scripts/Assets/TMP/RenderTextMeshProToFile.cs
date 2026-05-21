using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Assets.Scripts.Assets.TMP
{

    public class RenderTextMeshProToFile : MonoBehaviour
    {
        public GText TextToRender;

        public async Awaitable RenderInputTextAsync(string text, string assetFilePath, Color bgColor, int textureSize = 256)
        {
            await Awaitable.NextFrameAsync();

#if UNITY_EDITOR

            string fullFilePath = assetFilePath;

            if (fullFilePath.IndexOf(Application.dataPath) < 0)
            {
                fullFilePath = Application.dataPath + fullFilePath;
            }

            RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 32, RenderTextureFormat.ARGB32);
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.Create();

            string dataPath = Application.dataPath;

            TextToRender.transform.position = Vector3.zero;
            TextToRender.text = text;
            await Awaitable.NextFrameAsync();
            TextToRender.ForceMeshUpdate(true, true);
            await Awaitable.NextFrameAsync();
            Mesh mesh = TextToRender.mesh;

            Material mat = TextToRender.fontMaterial;

            RenderTexture oldActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, bgColor);

            GL.PushMatrix();
            GL.LoadOrtho();


            if (mesh == null)
            {
                Debug.Log("Mesh is missing vertices.");
                return;

            }
            Bounds b = mesh.bounds;

            // Scale to fit into 0–1 space
            float scale = 1.0f / Mathf.Max(b.size.x, b.size.y);

            // Move mesh to center (0.5, 0.5)
            Matrix4x4 m =
                Matrix4x4.TRS(
                    new Vector3(0.5f, 0.5f, 0),
                    Quaternion.identity,
                    Vector3.one * scale
                ) *
                Matrix4x4.TRS(-b.center, Quaternion.identity, Vector3.one);


            // 6. Draw the TMP mesh using its SDF material
            for (int i = 0; i < mat.passCount; i++)
            {
                mat.SetPass(i);
                Graphics.DrawMeshNow(mesh, m);
            }

            GL.PopMatrix();

            Texture2D newTex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            newTex.alphaIsTransparency = true;
            newTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            newTex.Apply();
            RenderTexture.active = oldActive;
            try
            {
                File.WriteAllBytes(fullFilePath, newTex.EncodeToPNG());

                string assetPath = fullFilePath.Replace(dataPath, "Assets");

                TextureImporter importer = TextureImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null)
                {
                    importer.alphaIsTransparency = true;

                    // 3. Save the changes to the meta file and re-import
                    importer.SaveAndReimport();

                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exc: " + ex.Message);
            }
            GameObject.DestroyImmediate(renderTexture);
            GameObject.DestroyImmediate(newTex);
#endif
        }
    }
}

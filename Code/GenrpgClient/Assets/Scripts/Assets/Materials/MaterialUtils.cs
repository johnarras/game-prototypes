using UnityEngine;
using UnityEngine.Rendering;

namespace OxDb.Client.Assets.Materials
{
    public static class MaterialUtils
    {
        public const string MainTexturePropertyName = "_BaseMap";
        public const string NormalMapPropertyName = "_BumpMap";
        public const string SmoothnessPropertyName = "_Smoothness";
        public const string BumpScalePropertyName = "_BumpScale";
        public const string BaseColorPropertyName = "_BaseColor";
        public const string EnableNormalMapKeyword = "_NORMALMAP";
        public const string SpecularColorPropertyName = "_SpecColor";
        public const string EmissionColorPropertyName = "_EmissionColor";

        public static readonly int BaseColorPropertyId = Shader.PropertyToID(BaseColorPropertyName);


        public static Texture2D GetMainTexture(Material mat)
        {
            return (Texture2D)mat.mainTexture;
        }

        public static Texture2D GetNormalMap(Material mat)
        {
            if (mat.HasProperty(NormalMapPropertyName))
            {
                return (Texture2D)mat.GetTexture(NormalMapPropertyName);
            }
            return null;
        }

        public static Material CreateTransparentVariant(Material sourceOpaqueMaterial)
        {
            // Create a perfect duplicate clone of the base material
            Material transparentMaterial = new Material(sourceOpaqueMaterial);
            transparentMaterial.name = sourceOpaqueMaterial.name + "_Transparent_Clone";

            // 1. Change Surface Type to Transparent (1 = Transparent, 0 = Opaque)
            transparentMaterial.SetFloat("_Surface", 1f);

            // 2. Set Blend Modes (SrcBlend = SrcAlpha, DstBlend = OneMinusSrcAlpha)
            transparentMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            transparentMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

            // 3. Turn off ZWrite (Standard transparency does not write to depth buffer)
            transparentMaterial.SetFloat("_ZWrite", 0f);

            // 4. Update URP Pipeline Render Queue tags
            transparentMaterial.renderQueue = (int)RenderQueue.Transparent;
            transparentMaterial.SetOverrideTag("RenderType", "Transparent");

            // 5. Enable/Disable the correct URP Shader Keywords
            transparentMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            transparentMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");

            // Disable Alpha Clipping features just in case they were active on the opaque source
            transparentMaterial.DisableKeyword("_ALPHATEST_ON");

            return transparentMaterial;
        }
    }
}

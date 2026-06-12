using UnityEngine;

namespace Assets.Scripts.Assets.Materials
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
        public const string EmissionColorPropertyName = "_Emission";


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
    }
}

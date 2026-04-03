using UnityEngine;

namespace Assets.Scripts.Assets.Materials
{
    public static class MaterialUtils
    {
        public const string NormalMapPropertyName = "_BumpMap";


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

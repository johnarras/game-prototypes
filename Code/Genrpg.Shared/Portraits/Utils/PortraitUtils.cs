using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Portraits.Utils
{
    public static class PortraitUtils
    {
        public static string GetFileSuffixFromIndex(int index)
        {
            return HashUtils.GetLowercaseAlphaIdFromVal(index);
        }
    }
}

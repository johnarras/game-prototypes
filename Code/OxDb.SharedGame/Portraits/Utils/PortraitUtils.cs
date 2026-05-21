using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Portraits.Utils
{
    public static class PortraitUtils
    {
        public static string GetFileSuffixFromIndex(int index)
        {
            return HashUtils.GetLowercaseAlphaIdFromVal(index);
        }
    }
}

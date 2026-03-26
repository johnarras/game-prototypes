namespace Genrpg.Shared.Input.Constants
{
    public class InputConstants
    {
        public const int MinActionIndex = 1;
        public const int MaxActionIndex = 10;
        public static bool OkActionIndex(long index)
        {
            return index >= MinActionIndex && index <= MaxActionIndex;
        }
    }
}



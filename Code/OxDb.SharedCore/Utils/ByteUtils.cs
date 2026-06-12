using System;

namespace OxDb.SharedCore.Utils
{
    public static class ByteUtils
    {
        public static byte[] ConcatenateArrays(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];

            first.AsSpan().CopyTo(result);
            second.AsSpan().CopyTo(result.AsSpan(first.Length));


            return result;
        }
    }
}

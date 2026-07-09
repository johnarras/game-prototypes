using System;
using System.Buffers;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OxDb.SharedCore.Utils
{
    public class HashUtils
    {
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString(); // only spot we should use this method
        }

        private static char[] _base58Chars = null;
        public static char[] GetBase58Chars()
        {
            if (_base58Chars != null)
            {
                return _base58Chars;
            }
            List<char> retval = new List<char>();
            for (int i = 0; i < 128; i++)
            {
                char c = (char)i;
                if (char.IsLetterOrDigit(c) && c != '0' && c != 'O' && c != 'l' && c != 'I')
                {
                    retval.Add(c);
                }
            }
            _base58Chars = retval.ToArray();
            return _base58Chars;
        }

        private static char[] _lowercaseAlphaChars = null;

        public static char[] GetLowercaseAlphaChars()
        {
            if (_lowercaseAlphaChars != null)
            {
                return _lowercaseAlphaChars;
            }

            List<char> retval = new List<char>();
            for (int i = (int)'a'; i <= (int)'z'; i++)
            {
                retval.Add((char)i);
            }
            _lowercaseAlphaChars = retval.ToArray();
            return _lowercaseAlphaChars;
        }

        public static byte[] GetRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }

        public static string GetLowercaseAlphaIdFromVal(long val)
        {
            char[] idChars = GetLowercaseAlphaChars();

            StringBuilder sb = new StringBuilder();

            long idval = val;

            while (idval > 0)
            {
                sb.Append(idChars[(int)(idval % idChars.Length)]);
                idval /= idChars.Length;
            }
            return sb.ToString();
        }

        public static string GetIdFromVal(long val)
        {
            char[] idChars = GetBase58Chars();

            StringBuilder sb = new StringBuilder();

            long idval = val;

            while (idval != 0)
            {
                sb.Append(idChars[(int)((idval % idChars.Length) + idChars.Length) % idChars.Length]);
                idval /= idChars.Length;
            }
            return sb.ToString();
        }

        public static string QuickHash(string txt)
        {
            MD5 algo = MD5.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            byte[] arr2 = algo.ComputeHash(arr);
            return ToHexString(arr2);
        }

        public static string ToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hex.Append(b.ToString("X2"));
            }
            return hex.ToString();
        }

        public static int QuickHash(ReadOnlySpan<char> txt, Span<byte> destination)
        {
            const int Md5HashSize = 16;

            if (destination.Length < Md5HashSize)
            {
                return -1;
            }

            int byteCount = Encoding.UTF8.GetByteCount(txt);
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> utf8Buffer = rentedArray.AsSpan(0, byteCount);

            try
            {
                Encoding.UTF8.GetBytes(txt, utf8Buffer);

                // Use an MD5 instance instead of the static method
                using (MD5 algo = MD5.Create())
                {
                    // .NET Standard 2.1 provides TryComputeHash on the instance
                    if (algo.TryComputeHash(utf8Buffer, destination, out int bytesWritten))
                    {
                        return bytesWritten;
                    }
                }

                return -1;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedArray, clearArray: true);
            }
        }

    }
}



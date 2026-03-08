using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Genrpg.Shared.Utils
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

        public static string GetIdFromVal(long val)
        {
            char[] idChars = GetBase58Chars();

            StringBuilder sb = new StringBuilder();

            long idval = val;

            while (idval > 0)
            {
                sb.Append(idChars[(int)(idval % idChars.Length)]);
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

        public static string GetnewBase58Id()
        {
            string encoded;
            do
            {
                Guid guid = Guid.NewGuid();

                ReadOnlySpan<byte> fullSpan = guid.ToByteArray();

                // Slice the span into two 8-byte segments

                long part1 = BitConverter.ToInt64(fullSpan.Slice(0, 8));
                long part2 = BitConverter.ToInt64(fullSpan.Slice(8, 8));

                encoded = HashUtils.GetIdFromVal(part1) + HashUtils.GetIdFromVal(part2);
            }
            while (IsInappropriate(encoded));

            return encoded;
        }

        // This needs to be improved obviously.
        private static readonly string[] _nameBlacklist = { 
            "fuck", "shit", "nazi", "cunt", 
            "piss", "slut", "nigg", "damn", 
            "hell", "asshole", "fuk", "shyt", 
            "coc", "dik", "vag",
        
        };
        private static bool IsInappropriate(string base58Id)
        {
            string lowerId = base58Id.ToLower();

            // Check for direct matches or leetspeak subs
            // You can expand this to check for '5' as 's', etc.
            string normalized = lowerId
                .Replace('5', 's')
                .Replace('1', 'i')
                .Replace('4', 'a')
                .Replace('8', 'b')
                .Replace('0', 'o')
                .Replace('3', 'e')
                .Replace('6', 'g')
                ;

            return _nameBlacklist.Any(word => normalized.Contains(word));
        }
    }
}



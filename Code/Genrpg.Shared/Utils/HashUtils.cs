using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Utils
{
    public class HashUtils
    {
        public static string NewUUId()
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
    }
}



using System;
using System.Collections.Generic;

namespace OxDb.SharedCore.Utils
{
    public static class LinqExtensionMethods
    {
        public static bool FastAny<T>(this List<T> list, Func<T, bool> predicate)
        {
            return FastFirstOrDefault(list, predicate) != null;
        }

        public static T FastFirstOrDefault<T>(this List<T> list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    return t;
                }
            }
            return default(T);
        }

        public static bool FastAny<T>(this T[] list, Func<T, bool> predicate)
        {
            return FastFirstOrDefault(list, predicate) != null;
        }

        public static T FastFirstOrDefault<T>(this T[] list, Func<T, bool> predicate)
        {
            foreach (T t in list)
            {
                if (predicate(t))
                {
                    return t;
                }
            }
            return default(T);
        }
    }
}



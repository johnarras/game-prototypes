using System;

namespace OxDb.SharedGame.DataStores.Utils
{
    public static class NoSqlUtils
    {
        public static string GetDocIdSuffix(Type t)
        {
            return "_" + t.Name;
        }

    }
}

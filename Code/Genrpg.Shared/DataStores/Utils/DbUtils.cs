namespace Genrpg.Shared.DataStores.Utils
{
    public static class DbUtils
    {
        public static string GetDbName(string category, string env)
        {
            return (env + "-" + category).ToLower();
        }
    }
}

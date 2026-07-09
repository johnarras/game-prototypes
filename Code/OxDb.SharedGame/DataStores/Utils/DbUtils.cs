namespace OxDb.SharedGame.DataStores.Utils
{
    public static class DbUtils
    {
        public static string GetDbName(string productName, string category, string env)
        {
            return (productName + "-" + category + "-" + env).ToLower();
        }
    }
}

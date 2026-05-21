namespace OxDb.SharedCore.Environments.Constants
{
    public class EnvNames
    {
        public const string Local = "local";
        public const string Dev = "dev";
        public const string Test = "test";
        public const string Staging = "staging";
        public const string Prod = "prod";


        public static bool IsProdEnv(string env)
        {
            if (string.IsNullOrEmpty(env))
            {
                return false;
            }
            return env.IndexOf(Prod) == 0 || env.IndexOf(Staging) == 0;
        }

        const string ProdSubName = "prod";
        const string DevSubName = "dev";

        public static string GetProdDevEnv(string env)
        {
            return IsProdEnv(env) ? ProdSubName : DevSubName;
        }
    }
}



using MessagePack;
namespace Genrpg.Shared.Constants
{

    public class Game
    {
        public const string Prefix = "Genrpg";
        public const string DefaultPrefix = "Genrpg";
    }

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
    }   
}



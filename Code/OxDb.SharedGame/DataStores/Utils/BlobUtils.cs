using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Environments.Constants;

namespace OxDb.SharedGame.DataStores.Utils
{
    public class BlobUtils
    {
        public static string GetBlobContainerName(string dataCategory, string gamePrefix, string env)
        {
            if (dataCategory.ToLower() == EDataCategories.Accounts.ToString().ToLower())
            {
                return "accounts";
            }

            string prodDevEnv = EnvNames.GetProdDevEnv(env);
            return (gamePrefix + "-" + prodDevEnv).ToLower();
        }

        public static string GetBlobSubfolder(string env, string clientVersion, string platformName)
        {
            if (env.ToLower() == EnvNames.Local.ToLower())
            {
                env = EnvNames.Dev.ToLower();
            }

            return (env + "/" + clientVersion + "/" + platformName).ToLower() + "/";
        }
    }
}



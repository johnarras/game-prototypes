using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Environments.Constants;

namespace OxDb.SharedGame.DataStores.Utils
{
    public class BlobUtils
    {

        const string topLevelContainerName = "assets/";

        public static string GetToplevelContainerName()
        {
            return topLevelContainerName;
        }

        public static string GetBlobContainerName(string gamePrefix, string env, string dataCategory)
        {
            if (env.ToLower() == EnvNames.Local.ToLower())
            {
                env = EnvNames.Dev.ToLower();
            }
            if (dataCategory.ToLower() == EDataCategories.Accounts.ToString().ToLower())
            {
                return topLevelContainerName + "platform/" + env + "/accounts";
            }
            return topLevelContainerName + gamePrefix.ToLower() + "/" + env.ToLower();
        }

        public static string GetBlobSubfolder(string clientVersion, string platformName)
        {

            return (clientVersion + "/" + platformName).ToLower() + "/";
        }
    }
}



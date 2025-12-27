using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.Shared.DataStores.Utils
{
    public class BlobUtils
    {
        public static string GetBlobContainerName(string dataCategory, string gamePrefix, string env)
        {
            if (dataCategory.ToLower() == EDataCategories.Accounts.ToString().ToLower())
            {
                return "accounts";
            }
            if (env == EnvNames.Local.ToLower())
            {
                env = EnvNames.Dev.ToLower();
            }
            return (gamePrefix + env).ToLower();
        }
    }
}



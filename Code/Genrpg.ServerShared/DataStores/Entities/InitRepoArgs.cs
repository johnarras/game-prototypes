using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.ServerShared.DataStores.Entities
{
    public class InitRepoArgs
    {
        public ERepoTypes RepoType { get; set; }
        public EDataCategories Category { get; set; }
        public string Env { get; set; }
    }
}

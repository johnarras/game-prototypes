using OxDb.SharedCore.DataStores.DataGroups;

namespace OxDb.ServerCore.AzureImpl.DataStores.Entities
{
    public class InitRepoArgs
    {
        public ERepoTypes RepoType { get; set; }
        public EDataCategories Category { get; set; }
        public string Env { get; set; }
        public string ProductName { get; set; }
    }
}



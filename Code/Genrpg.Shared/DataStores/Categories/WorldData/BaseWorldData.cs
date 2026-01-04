using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.WorldData
{
    [DataGroup(EDataCategories.Worlds, ERepoTypes.Mongo)]
    public abstract class BaseWorldData : ISearchableItem
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        public abstract void Delete(IRepositoryService repoSystem);
    }
}



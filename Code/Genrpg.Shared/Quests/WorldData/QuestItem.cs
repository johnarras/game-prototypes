using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Quests.WorldData
{
    public class QuestItem : BaseWorldData, IIndexedGameItem, IMapOwnerId
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        public override string Id { get; set; }
        public string OwnerId { get; set; }
        public string MapId { get; set; }
        public long IdKey { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }


    }
}



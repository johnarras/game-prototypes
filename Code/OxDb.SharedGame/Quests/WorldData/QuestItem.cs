using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.WorldData;
using OxDb.SharedGame.Interfaces;

namespace OxDb.SharedGame.Quests.WorldData
{
    public class QuestItem : BaseWorldData, IIndexedGameItem, IMapOwnerId
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        public override string Id { get; set; }
        public string OwnerId { get; set; }
        public string MapId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }


    }
}



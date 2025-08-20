using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Constants;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Quests.Helpers
{
    public class ExploreCrawlerQuestTypeHelper : BaseCrawlerQuestTypeHelper
    {
        public override long Key => CrawlerQuestTypes.ExploreMap;

        protected override string QuestVerb => "Fully Explore";

        public override async Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap,
                MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {

            CrawlerQuest quest = new CrawlerQuest()
            {
                CrawlerMapId = targetMap.Map.BaseCrawlerMapId,
                TargetEntityId = targetMap.Map.BaseCrawlerMapId,
                CrawlerQuestTypeId = CrawlerQuestTypes.ExploreMap,
                IdKey = CollectionUtils.GetNextIdKey(world.Quests),
                Name = "Fully Explore a Level in " + targetMap.Map.Name,
                StartCrawlerNpcId = npc.IdKey,
                EndCrawlerNpcId = npc.IdKey,
                Quantity = 1,
                TargetSingularName = "a level in " + targetMap.Map.Name,
                TargetPluralName = "a level in " + targetMap.Map.Name,
            };

            world.Quests.Add(quest);

            await Task.CompletedTask;
            return;
        }

        public override async Task<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool showFullDescription, bool showCurrentStatus, bool showNPC)
        {
            return await base.ShowQuestStatus(party, crawlerQuestId, false, true, showNPC);
        }
    }
}

using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Constants;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Quests.Helpers
{
    public class ExploreCrawlerQuestTypeHelper : BaseCrawlerQuestTypeHelper
    {
        public override long HelperKey => CrawlerQuestTypes.ExploreMap;

        protected override string QuestVerb => "Fully Explore";

        public override async Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap,
                MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {


            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                // 1 explore quest in crawler mode at once and only rarely
                if (rand.NextDouble() > 0.1f || world.Quests.FastAny(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.ExploreMap))
                {
                    return;
                }
            }

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



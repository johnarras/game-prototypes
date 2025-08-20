using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Constants;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Quests.Helpers
{
    public class TravelCrawlerQuestTypeHelper : BaseCrawlerQuestTypeHelper
    {
        protected override string QuestVerb => "Take a Message to";

        public override long Key => CrawlerQuestTypes.TravelToNpc;

        public override async Task SetupQuest(PartyData party, CrawlerWorld world,
            CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {
            List<CrawlerNpc> allNpcs = world.Npcs.Where(n => n.MapId != npc.MapId).OrderBy(x => Math.Abs(x.Level - npc.Level)).ToList();

            if (allNpcs.Count < 1)
            {
                return;
            }

            int maxSearch = Math.Min(allNpcs.Count, 5);

            CrawlerNpc otherNPC = allNpcs[rand.Next(allNpcs.Count)];

            CrawlerQuest quest = new CrawlerQuest()
            {
                CrawlerMapId = otherNPC.MapId,
                TargetEntityId = otherNPC.IdKey,
                CrawlerQuestTypeId = CrawlerQuestTypes.TravelToNpc,
                IdKey = CollectionUtils.GetNextIdKey(world.Quests),
                Name = "Take a message to " + otherNPC.Name,
                StartCrawlerNpcId = npc.IdKey,
                EndCrawlerNpcId = otherNPC.IdKey,
                Quantity = 1,
                TargetSingularName = otherNPC.Name,
                TargetPluralName = otherNPC.Name,
            };

            world.Quests.Add(quest);
            await Task.CompletedTask;
            return;
        }

        public override async Task<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool fullDescription, bool showCurrentStatus, bool showNPC)
        {
            return await base.ShowQuestStatus(party, crawlerQuestId, false, showCurrentStatus, showNPC);
        }
    }
}

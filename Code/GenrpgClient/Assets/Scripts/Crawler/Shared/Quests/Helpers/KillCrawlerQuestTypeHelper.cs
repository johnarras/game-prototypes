using Assets.Scripts.Crawler.MapGen.Helpers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Constants;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Quests.Helpers
{
    public class KillCrawlerQuestTypeHelper : BaseCrawlerQuestTypeHelper
    {

        private ICrawlerQuestService _questService = null;

        public override long HelperKey => CrawlerQuestTypes.KillMonsters;

        protected override string QuestVerb => "Kill";

        public override async Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap,
            MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {
            CrawlerMapSettings mapService = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            CrawlerMap baseMap = world.GetMap(targetMap.Map.BaseCrawlerMapId);

            List<ZoneUnitSpawn> startSpawns = await _worldService.GetSpawnsAtPoint(party, targetMap.Map.IdKey,
                targetMap.Link.ToX, targetMap.Link.ToZ);

            if (startSpawns.Count < 1)
            {
                return;
            }

            List<long> killEntities = world.Quests.Where(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.KillMonsters).Select(x => x.TargetEntityId).ToList();

            List<ZoneUnitSpawn> okSpawns = startSpawns.Where(x => !killEntities.Contains(x.UnitTypeId)).ToList();

            if (okSpawns.Count < 1)
            {
                okSpawns = startSpawns;
            }


            List<long> sameDungeonKillQuestUnitIds = (await _questService.GetQuestsForMap(party, party.CurrPos.MapId)).Where(x => x.CrawlerQuestTypeId == CrawlerQuestTypes.KillMonsters)
                .Select(x => x.TargetEntityId).Distinct().ToList();


            okSpawns = okSpawns.Where(x => !sameDungeonKillQuestUnitIds.Contains(x.UnitTypeId)).ToList();

            if (okSpawns.Count < 1)
            {
                okSpawns = startSpawns;
            }

            if (okSpawns.Count > mapService.SharedZoneUnitCount)
            {
                okSpawns = okSpawns.Take(mapService.SharedZoneUnitCount).ToList();
            }

            ZoneUnitSpawn spawn = okSpawns[rand.Next(okSpawns.Count)];

            UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(spawn.UnitTypeId);

            if (unitType == null)
            {
                return;
            }

            long quantity = GetMaxQuantity(party, npc.Level, rand);

            CrawlerQuest quest = new CrawlerQuest()
            {
                CrawlerMapId = targetMap.Map.BaseCrawlerMapId,
                CrawlerQuestTypeId = CrawlerQuestTypes.KillMonsters,
                IdKey = CollectionUtils.GetNextIdKey(world.Quests),
                Name = "Eliminate " + quantity + " " + unitType.PluralName + " in " + baseMap.Name,
                StartCrawlerNpcId = npc.IdKey,
                EndCrawlerNpcId = npc.IdKey,
                Quantity = quantity,
                TargetEntityId = unitType.IdKey,
                TargetSingularName = unitType.Name,
                TargetPluralName = unitType.PluralName,
            };

            world.AddQuest(quest);
            await Task.CompletedTask;
            return;
        }
    }
}



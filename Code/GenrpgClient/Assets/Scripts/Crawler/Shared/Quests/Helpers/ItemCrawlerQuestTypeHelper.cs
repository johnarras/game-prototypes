using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Loot.Services;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Constants;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Inventory.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Quests.Helpers
{
    public class ItemCrawlerQuestTypeHelper : BaseCrawlerQuestTypeHelper
    {

        private ILootGenService _lootGenService = null;

        protected override string QuestVerb => "Collect";

        public override long HelperKey => CrawlerQuestTypes.LootItems;

        public override async Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap,
            MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {
            CrawlerMapSettings mapService = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            CrawlerMap baseMap = world.GetMap(targetMap.Map.BaseCrawlerMapId);

            ItemNameResult result = _lootGenService.GenerateItemNames(rand, 1, 1).First();

            long quantity = GetMaxQuantity(party, npc.Level, rand);

            CrawlerQuest quest = new CrawlerQuest()
            {
                CrawlerMapId = targetMap.Map.BaseCrawlerMapId,
                CrawlerQuestTypeId = CrawlerQuestTypes.LootItems,
                IdKey = CollectionUtils.GetNextIdKey(world.Quests),
                Name = "Collect " + quantity + " " + result.PluralName + " in " + baseMap.Name,
                StartCrawlerNpcId = npc.IdKey,
                EndCrawlerNpcId = npc.IdKey,
                Quantity = quantity,
                TargetSingularName = result.SingularName,
                TargetPluralName = result.PluralName
            };

            world.AddQuest(quest);
            await Task.CompletedTask;
            return;
        }
    }
}



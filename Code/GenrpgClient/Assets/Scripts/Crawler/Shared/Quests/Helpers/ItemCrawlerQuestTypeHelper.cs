using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Constants;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Utils;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Quests.Helpers
{
    public class ItemCrawlerQuestTypeHelper : BaseCrawlerQuestTypeHelper
    {

        private ILootGenService _lootGenService = null;

        protected override string QuestVerb => "Collect";

        public override long Key => CrawlerQuestTypes.LootItems;

        public override async Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap,
            MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token)
        {
            CrawlerMapSettings mapService = _gameData.Get<CrawlerMapSettings>(_gs.ch);

            CrawlerMap baseMap = world.GetMap(targetMap.Map.BaseCrawlerMapId);

            ItemNameResult result = _lootGenService.GenerateItemNames(rand, 1, 1).First();

            long quantity = GetMaxQuantity(npc.Level, rand);

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

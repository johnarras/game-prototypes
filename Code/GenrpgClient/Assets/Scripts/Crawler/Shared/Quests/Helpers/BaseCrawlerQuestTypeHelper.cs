using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Constants;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Quests.Helpers
{
    public abstract class BaseCrawlerQuestTypeHelper : ICrawlerQuestTypeHelper
    {
        protected ICrawlerWorldService _worldService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ICrawlerMapService _mapService = null;
        protected ILogService _logService = null;
        protected ITextService _textService = null;
        protected ICrawlerCombatService _combatService = null;

        public abstract long Key { get; }
        protected abstract string QuestVerb { get; }
        public abstract Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, 
            MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token);

        protected CrawlerQuestType GetQuestType()
        {
            return _gameData.Get<CrawlerQuestSettings>(_gs.ch).Get(Key);
        }

        protected virtual long GetMaxQuantity(long npcLevel, IRandom rand)
        {
            double monsterScale = GetQuestType().MonsterGroupSizeScale;

            if (monsterScale == 0)
            {
                return 1;
            }
                
            long maxGroupSize = _combatService.GetMaxGroupSize(npcLevel);
            return MathUtils.LongRange(maxGroupSize / 2 + 1, maxGroupSize * 3 / 2 + 1, rand);
        }

        public virtual async Task<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool showFullDescription, bool showCurrentStatus, bool showNPC)
        {

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerQuest quest = world.GetQuest(crawlerQuestId);

            if (quest == null)
            {
                return "Unknown Quest";
            }
            CrawlerNpc startNpc = world.GetNpc(quest.StartCrawlerNpcId);

            string startMapName = _mapService.GetMapName(party, startNpc.MapId, startNpc.X, startNpc.Z);

            string startNpcInfo = startNpc.Name + " in " + startMapName;

            string endNpcInfo = "and return to them for a reward.";
           
            CrawlerNpc endNpc = world.GetNpc(quest.EndCrawlerNpcId);

            if (endNpc != startNpc)
            {
                endNpcInfo = " and go to " + endNpc.Name + " in " + _mapService.GetMapName(party, endNpc.MapId, endNpc.X, endNpc.Z) +
                    " for a reward";
            }


            StringBuilder sb = new StringBuilder();

            if (showNPC)
            {
                sb.Append(startNpcInfo + " wants you to ");
            }

            sb.Append(QuestVerb + " ");
            if (showFullDescription)
            {
                sb.Append(quest.Quantity + " ");
            }

            sb.Append(quest.Quantity > 1 ? quest.TargetPluralName : quest.TargetSingularName);
            
            CrawlerMap map = world.GetMap(quest.CrawlerMapId);
            if (map != null)
            {
                sb.Append(" in " + map.Name + " ");
            }
            
            if (showCurrentStatus)
            {
                string currText = sb.ToString();
                PartyQuest partyQuest = party.Quests.FirstOrDefault(x=>x.CrawlerQuestId == crawlerQuestId);
                if (party.CompletedQuests.HasBit(quest.IdKey) ||
                    (partyQuest != null && partyQuest.CurrQuantity >= quest.Quantity))
                {
                    sb.Clear();
                    sb.Append(_textService.HighlightText("(Complete!) ", TextColors.ColorGold) + currText);
                }
                else if (partyQuest != null)
                {
                    sb.Clear();
                    sb.Append(_textService.HighlightText("(" + partyQuest.CurrQuantity + "/" + quest.Quantity + ")", TextColors.ColorGold) + " "
                        + currText);
                }
            }
            if (showNPC)
            {
                sb.Append(endNpcInfo);
            }
            return sb.ToString();
        }
    }
}

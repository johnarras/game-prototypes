using Assets.Scripts.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Services;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Quests.Helpers
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
        protected ICrawlerOptionsService _optionsService = null;

        public abstract long HelperKey { get; }
        protected abstract string QuestVerb { get; }
        public abstract Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap,
            MapLink targetMap, CrawlerNpc npc, CrawlerQuestType questType, IRandom rand, CancellationToken token);

        protected CrawlerQuestType GetQuestType()
        {
            return _gameData.Get<CrawlerQuestSettings>(_gs.ch).Get(HelperKey);
        }

        protected virtual long GetMaxQuantity(PartyData party, long npcLevel, IRandom rand)
        {
            double monsterScale = GetQuestType().MonsterGroupSizeScale;

            if (monsterScale == 0)
            {
                return 1;
            }

            long maxGroupSize = _combatService.GetMaxGroupSize(party, npcLevel);
            return RandUtils.LongRange(maxGroupSize / 2 + 1, maxGroupSize * 3 / 2 + 1, rand);
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

            string endNpcInfo = " and return to them for a reward.";

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
                PartyQuest partyQuest = party.Quests.FirstOrDefault(x => x.CrawlerQuestId == crawlerQuestId);
                if (party.CompletedQuests.HasBitIndex(quest.IdKey) ||
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



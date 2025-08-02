using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Entities;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.NpcsQuests
{
    public class NpcMainStateHelper : BuildingStateHelper
    {

        private ICrawlerQuestService _questService = null;

        public override ECrawlerStates Key => ECrawlerStates.NpcMain;
        public override long TriggerBuildingId() { return BuildingTypes.Npc; }
        public override long TriggerDetailEntityTypeId() { return EntityTypes.Npc; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            MapCellDetail currNpcDetail = action.ExtraData as MapCellDetail;

            if (currNpcDetail == null)
            {
                return ShowEmptyBuilding(stateData);
            }

            PartyData party = _crawlerService.GetParty();

            stateData.BGSpriteName = CrawlerClientConstants.HouseImage + GetBuildingImageIndex(party, TriggerBuildingId()); 

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerNpc npc = world.GetNpc(currNpcDetail.EntityId);

            if (npc == null)
            {
                return ShowEmptyBuilding(stateData);
            }

            stateData.WorldSpriteName = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(npc.UnitTypeId).Icon;

            stateData.AddText("Hello brave adventurers");
            stateData.AddText("I am " + npc.Name + ".");
            stateData.AddText("How may I assist you?");

            List<CrawlerQuest> startQuests = world.Quests.Where(x=>x.StartCrawlerNpcId == npc.IdKey).ToList();
            List<CrawlerQuest> endQuests = world.Quests.Where(x => x.EndCrawlerNpcId == npc.IdKey).ToList();
            List<CrawlerQuest> allQuests = startQuests.Concat(endQuests).Distinct().OrderBy(x => x.IdKey).ToList();

            List<FullQuest> availableQuests = new List<FullQuest>();

            List<FullQuest> currentQuests = new List<FullQuest>();

            foreach (CrawlerQuest quest in allQuests)
            {
                if (party.CompletedQuests.HasBit(quest.IdKey))
                {
                    continue;
                }

                PartyQuest partyQuest = party.Quests.FirstOrDefault(x=>x.CrawlerQuestId == quest.IdKey);    

                if (partyQuest == null && quest.StartCrawlerNpcId == npc.IdKey)
                {
                    availableQuests.Add(new FullQuest() { Quest = quest, ReturnState = ECrawlerStates.NpcMain, NpcDetail = currNpcDetail });
                }
                else if (partyQuest != null && quest.EndCrawlerNpcId == npc.IdKey)
                {
                    currentQuests.Add(new FullQuest()
                    {
                        Quest = quest,
                        Progress = partyQuest,
                        NpcDetail = currNpcDetail,
                        ReturnState = ECrawlerStates.NpcMain,
                    });
                }
            }

            if (availableQuests.Count > 0)
            {
                stateData.AddText("Available Quests:");

                foreach (FullQuest fullQuest in availableQuests)
                {
                    stateData.Actions.Add(new CrawlerStateAction(" --> " + 
                        await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, true, false,false),
                        CharCodes.None, ECrawlerStates.QuestDetail, null,
                     fullQuest));
                }
            }

            if (currentQuests.Count > 0)
            {
                stateData.AddText("Quests in Progress: ");

                foreach (FullQuest fullQuest in currentQuests)
                {
                    // Is complete.
                    if (fullQuest.Progress != null && fullQuest.Progress.CurrQuantity >= fullQuest.Quest.Quantity)
                    {
                        stateData.Actions.Add(new CrawlerStateAction(
                            await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, false, true, false),
                            CharCodes.None, ECrawlerStates.NpcMain,
                            () =>
                            {
                                _questService.CompleteQuest(party, fullQuest, token);
                            }, extraData: currNpcDetail));
                    }
                    else
                    {
                        stateData.Actions.Add(new CrawlerStateAction
                            (await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, false, true, false),
                                CharCodes.None, ECrawlerStates.QuestDetail, null, fullQuest
                                ));
                    }
                }
            }

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", CharCodes.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }

        CrawlerStateData ShowEmptyBuilding(CrawlerStateData stateData)
        {
            stateData.AddText("This building is empty...");

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", CharCodes.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}

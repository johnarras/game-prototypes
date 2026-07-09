using Assets.Scripts.UI.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Entities;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.NpcsQuests
{
    public class NpcMainStateHelper : BuildingStateHelper
    {

        private ICrawlerQuestService _questService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.NpcMain;
        public override long TriggerBuildingId() { return BuildingTypes.Npc; }
        public override long TriggerDetailEntityTypeId() { return EntityTypes.Npc; }

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            MapCellDetail currNpcDetail = action.ExtraData as MapCellDetail;

            if (currNpcDetail == null)
            {
                return ShowEmptyBuilding(stateData);
            }

            PartyData party = _crawlerService.GetParty();

            stateData.BGSpriteName = CrawlerClientConstants.BuildingImage;

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

            NPCQuestStatus npcQuestStatus = await _questService.GetNpcQuestStatus(party, world, npc.IdKey, currNpcDetail, token);

            if (npcQuestStatus.AvailableQuests.Count > 0)
            {
                stateData.AddText(_textService.HighlightText("Available Quests:", TextColors.ColorGold));

                foreach (FullQuest fullQuest in npcQuestStatus.AvailableQuests)
                {
                    stateData.Actions.Add(new CrawlerStateAction(" --> " +
                        await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, true, false, false),
                        Key.None, ECrawlerStates.QuestDetail, null,
                     fullQuest));
                }
            }

            if (npcQuestStatus.CurrentQuests.Count > 0)
            {
                stateData.AddText(_textService.HighlightText("Quests in Progress: ", TextColors.ColorGold));

                foreach (FullQuest fullQuest in npcQuestStatus.CurrentQuests)
                {
                    // Is complete.
                    if (fullQuest.Progress != null && fullQuest.Progress.CurrQuantity >= fullQuest.Quest.Quantity)
                    {
                        stateData.Actions.Add(new CrawlerStateAction(
                            await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, false, true, false),
                            Key.None, ECrawlerStates.NpcMain,
                            () =>
                            {
                                _questService.CompleteQuest(party, fullQuest, token);
                            }, extraData: currNpcDetail));
                    }
                    else
                    {
                        stateData.Actions.Add(new CrawlerStateAction
                            (await _questService.ShowQuestStatus(party, fullQuest.Quest.IdKey, false, true, false),
                                Key.None, ECrawlerStates.QuestDetail, null, fullQuest
                                ));
                    }
                }
            }

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", Key.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }

        CrawlerStateData ShowEmptyBuilding(CrawlerStateData stateData)
        {
            stateData.AddText("This building is empty...");

            stateData.Actions.Add(new CrawlerStateAction("Back to the city", Key.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}



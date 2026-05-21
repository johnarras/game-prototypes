using Assets.Scripts.Crawler.Maps.ClientEvents;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Riddles.Settings;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.EntranceRiddles
{
    public class RiddleStateHelper : BaseStateHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.Riddle;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            CrawlerMoveStatus moveStatus = action.ExtraData as CrawlerMoveStatus;

            CrawlerStateData errorState = new CrawlerStateData(ECrawlerStates.ExploreWorld, true);

            if (moveStatus == null)
            {
                return errorState;
            }

            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            int riddleIndex = map.GetEntityId(moveStatus.EX, moveStatus.EZ, EntityTypes.Riddle);

            if (riddleIndex < 1)
            {
                return errorState;
            }

            RiddleHint hint = map.RiddleHints?.Hints.FirstOrDefault(x => x.Index == riddleIndex) ?? null;

            if (hint == null || string.IsNullOrEmpty(hint.Text))
            {
                return errorState;
            }

            string[] lines = hint.Text.Split("\n");

            for (int l = 0; l < lines.Length; l++)
            {
                stateData.AddText("\"" + lines[l] + "\"");
                stateData.AddBlankLine();

            }

            stateData.AddBlankLine();


            RiddleType rtype = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(map.RiddleHints?.RiddleTypeId ?? 0);

            CrawlerMapStatus status = party.GetMapStatus(party.CurrPos.MapId, false);

            if (status == null || rtype == null || !rtype.IsToggle)
            {
                AddSpaceAction(stateData);
            }
            else
            {
                bool isOn = party.HasRiddleBitIndex(riddleIndex);

                string currState = isOn ? "On" : "Off";
                string oppState = isOn ? "Off" : "On";

                stateData.AddText("The orb is " + currState);
                stateData.AddText("Do you want to turn it " + oppState);
                stateData.Actions.Add(new CrawlerStateAction("Yes turn it " + oppState, Key.Y, ECrawlerStates.Riddle,
                    () =>
                    {
                        if (isOn)
                        {
                            party.RemoveRiddleBitIndex(riddleIndex);
                        }
                        else
                        {
                            party.AddRiddleBitIndex(riddleIndex);
                        }
                        _dispatcher.Dispatch(new RedrawMapCell() { X = party.CurrPos.X, Z = party.CurrPos.Z });
                    }, moveStatus));
                stateData.Actions.Add(new CrawlerStateAction("No leave it alone.", Key.N, ECrawlerStates.ExploreWorld, null));
            }
            AddSpaceAction(stateData);
            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}



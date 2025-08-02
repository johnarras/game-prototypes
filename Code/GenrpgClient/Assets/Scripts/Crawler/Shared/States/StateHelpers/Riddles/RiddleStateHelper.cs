using Assets.Scripts.Crawler.Maps.ClientEvents;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Errors;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Utils;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.EntranceRiddles
{
    public class RiddleStateHelper : BaseStateHelper
    {
        public override ECrawlerStates Key => ECrawlerStates.Riddle;

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
                stateData.AddText(" \n");
            }

            stateData.AddText(" \n");

            RiddleType rtype = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(map.RiddleHints?.RiddleTypeId ?? 0);

            CrawlerMapStatus status = party.GetMapStatus(party.CurrPos.MapId, false);

            if (status == null || rtype == null || !rtype.IsToggle)
            {
                AddSpaceAction(stateData);
            }
            else
            {
                bool isOn = FlagUtils.IsSet(party.RiddleStatus, 1 << riddleIndex);

                string currState = isOn ? "On" : "Off";
                string oppState = isOn ? "Off" : "On";

                stateData.AddText("The orb is " + currState);
                stateData.AddText("Do you want to turn it " + oppState);
                stateData.Actions.Add(new CrawlerStateAction("Yes turn it " + oppState, 'Y', ECrawlerStates.Riddle,
                    () =>
                    {
                        if (isOn)
                        {
                            party.RiddleStatus &= ~(1 << riddleIndex);
                        }
                        else
                        {
                            party.RiddleStatus |= (long)(1 << riddleIndex);
                        }
                        _dispatcher.Dispatch(new RedrawMapCell() { X = party.CurrPos.X, Z = party.CurrPos.Z });
                    }, moveStatus));
                stateData.Actions.Add(new CrawlerStateAction("No leave it alone.", 'N', ECrawlerStates.ExploreWorld, null));
            }
            AddSpaceAction(stateData);
            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));
            
            await Task.CompletedTask;
            return stateData;
        }
    }
}

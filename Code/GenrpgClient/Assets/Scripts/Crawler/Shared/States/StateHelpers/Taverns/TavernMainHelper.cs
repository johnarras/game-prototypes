using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.Tavern.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Taverns
{
    public class TavernMainHelper : BuildingStateHelper
    {
        private ITimeOfDayService _timeService = null;
        private ITavernService _tavernService = null;
        public override ECrawlerStates HelperKey => ECrawlerStates.TavernMain;
        public override long TriggerBuildingId() { return BuildingTypes.Tavern; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.BGSpriteName = CrawlerClientConstants.BuildingImage;

            PartyData party = _crawlerService.GetParty();

            int index = (party.CurrPos.X * 11 + party.CurrPos.Z * 31) % 5;

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            if (action.ExtraData is string prevText)
            {
                stateData.Actions.Add(new CrawlerStateAction(prevText));
            }


            stateData.Actions.Add(new CrawlerStateAction("Eat", Key.E, ECrawlerStates.TavernMain,
            () =>
                {
                    _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Eat);
                },
            "You have a delicious meal!"));

            stateData.Actions.Add(new CrawlerStateAction("Drink", Key.D, ECrawlerStates.TavernMain,
            () =>
            {
                _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Drink);
            },
            "You enjoy your drink!"));

            stateData.Actions.Add(new CrawlerStateAction("Rumor", Key.R, ECrawlerStates.TavernMain,
            () =>
            {
                _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Rumor);
            },
                _textService.HighlightText("Someone whispers...\n\"" + _tavernService.GetRumor(party, world) + "\"", TextColors.ColorCyan)
            ));

            stateData.Actions.Add(new CrawlerStateAction("Exit", Key.Escape, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;
        }
    }
}



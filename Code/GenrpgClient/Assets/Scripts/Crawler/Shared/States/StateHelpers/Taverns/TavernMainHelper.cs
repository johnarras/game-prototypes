using Assets.Scripts.UI.Constants;
using OxDb.SharedGame.Buildings.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Buildings;
using OxDb.SharedGame.Crawler.Tavern.Services;
using OxDb.SharedGame.Crawler.TimeOfDay.Constants;
using OxDb.SharedGame.Crawler.TimeOfDay.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Taverns
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



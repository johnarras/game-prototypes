using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Loot.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Exploring
{

    public class LevelMapStateHelper : BaseStateHelper
    {

        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.LevelMap;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            stateData.BGImageOnly = true;
            stateData.BGSpriteName = CrawlerClientConstants.TreasureImage;

            PartyData party = _crawlerService.GetParty();

            bool isComplete = party.CompletedMaps.HasBitIndex(party.CurrPos.MapId);

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            if (!isComplete)
            {
                stateData.AddText("You have found a map of this area.");
                stateData.AddText("Would you like to examine it?");

                stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.ExploreWorld,

                    () =>
                    {
                        _mapService.SetMapComplete(party, world, party.CurrPos.MapId);
                        party.LastAutoCompleteLevel = party.CurrPos.MapId;
                    }));

                stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.ExploreWorld));


            }
            else
            {
                stateData.AddText("The level map is here.");
            }


            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));


            return stateData;
        }
    }
}



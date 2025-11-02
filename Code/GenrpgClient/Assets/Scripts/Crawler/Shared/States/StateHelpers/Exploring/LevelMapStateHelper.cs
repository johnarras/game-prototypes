using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{

    public class LevelMapStateHelper : BaseStateHelper
    {

        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates Key => ECrawlerStates.LevelMap;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            stateData.BGImageOnly = true;
            stateData.BGSpriteName = CrawlerClientConstants.TreasureImage;

            PartyData party = _crawlerService.GetParty();

            bool isComplete = party.CompletedMaps.HasBit(party.CurrPos.MapId);

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            if (!isComplete)
            {
                stateData.AddText("You have found a map of this area.");
                stateData.AddText("Would you like to examine it?");

                stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld,

                    () =>
                    {
                        _mapService.SetMapComplete(party, world, party.CurrPos.MapId);
                        party.LastAutoCompleteLevel = party.CurrPos.MapId;
                    }));

                stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));


            }
            else
            {
                stateData.AddText("The level map is here.");
            }


            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));


            return stateData;
        }
    }
}

using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class ReturnToSafetyHelper : BaseStateHelper
    {
        private ITextSerializer _serializer;
        public override ECrawlerStates Key => ECrawlerStates.ReturnToSafety;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);
            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            if (map.CrawlerMapTypeId == CrawlerMapTypes.City)
            {
                CrawlerMap returnMap = _worldService.GetMap(party.RecallPos.MapId);
                if (returnMap != null)
                {
                    stateData.AddText("Return to " + returnMap.Name + "?");
                    stateData.AddText("You won't be able to return to town this way");
                    stateData.AddText("until you fully explore another dungeon level.");

                    EnterCrawlerMapData mapData = new EnterCrawlerMapData()
                    {
                        Map = returnMap,
                        MapId = party.RecallPos.MapId,
                        MapX = party.RecallPos.X,
                        MapZ = party.RecallPos.Z,
                        MapRot = party.RecallPos.Rot,
                    };

                    stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld,
                        () =>
                        {
                            party.RecallPos = new MapPosition();
                        }, mapData));

                    stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));

                }
                else
                {
                    stateData.AddText("No return map found.");
                }

            }
            else
            {
                if (party.HasFlag(PartyFlags.HasRecall))
                {
                    stateData.AddText("Do you wish to return to the starting city?");
                    stateData.AddText("You may do this one time per completed level.");
                    stateData.AddText("However, these recalls do not accumulate.");
                    stateData.AddText("Consider using town portal instead.");

                    EnterCrawlerMapData safetyData = new EnterCrawlerMapData()
                    {
                        ReturnToSafety = true,
                    };

                    stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld,
                        () =>
                        {
                            party.RecallPos = _serializer.MakeCopy(party.CurrPos);
                            party.RemoveFlags(PartyFlags.HasRecall);
                        }, safetyData));

                    stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));

                }
                else
                {
                    stateData.AddText("You cannot use this ability until you completely");
                    stateData.AddText("explore another dungeon level.");
                }
            }
               

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}

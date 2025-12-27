using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Spells.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class BeaconSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.Beacon;

        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            PartyMember member = action.Action.Member;

            CrawlerMap beaconMap = world.GetMap(member.BeaconMapId);

            if (beaconMap != null)
            {
                int mapx = action.Action.Member.BeaconMapX;
                int mapz = action.Action.Member.BeaconMapZ;

                bool locationIsOk = true;
                if (mapx < 0 || mapx >= beaconMap.Width ||
                    mapz < 0 || mapz >= beaconMap.Height ||
                    beaconMap.Get(mapx, mapz, CellIndex.Terrain) == 0)
                {
                    locationIsOk = false;
                }

                if (locationIsOk)
                {
                    EnterCrawlerMapData mapData = new EnterCrawlerMapData()
                    {
                        MapId = member.BeaconMapId,
                        MapX = member.BeaconMapX,
                        MapZ = member.BeaconMapZ,
                        MapRot = member.BeaconRot,
                        World = world,
                        Map = beaconMap,
                    };

                    string txt = "Return to Beacon At " + beaconMap.Name + " (" + member.BeaconMapX + "," + member.BeaconMapZ + ")";


                    stateData.Actions.Add(new CrawlerStateAction(txt, Key.R, ECrawlerStates.ExploreWorld, extraData: mapData));
                    stateData.AddBlankLine();
                }
            }

            stateData.Actions.Add(new CrawlerStateAction("Set Beacon Target", Key.S, ECrawlerStates.ExploreWorld, () =>
            {
                member.BeaconMapId = party.CurrPos.MapId;
                member.BeaconMapX = party.CurrPos.X;
                member.BeaconMapZ = party.CurrPos.Z;
                member.BeaconRot = party.CurrPos.Rot;
            }));
            stateData.AddBlankLine();

            stateData.Actions.Add(new CrawlerStateAction("Back to Exploration.", Key.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}



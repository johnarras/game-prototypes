using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Constants;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Settings.SpecialMagic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class RevealAreaSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.RevealArea;

        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            double utilityTier = _roleService.GetRoleScalingLevel(party, action.Action.Member, RoleScalingTypes.Utility);

            int radius = (int)Math.Floor(Math.Sqrt(utilityTier));

            int cx = party.CurrPos.X;
            int cz = party.CurrPos.Z;

            for (int x = cx - radius; x <= cx + radius; x++)
            {
                int nx = x;

                if (nx < 0 || nx >= map.Width)
                {
                    if (!map.HasFlag(CrawlerMapFlags.IsLooping))
                    {
                        continue;
                    }
                    nx = (nx + map.Width) % map.Width;
                }
                for (int z = cz - radius; z <= cz + radius; z++)
                {
                    int nz = z;
                    if (nz < 0 || nz >= map.Height)
                    {
                        if (!map.HasFlag(CrawlerMapFlags.IsLooping))
                        {
                            continue;
                        }
                        nz = (nz + map.Height) % map.Height;
                    }

                    if (map.Get(nx, nz, CellIndex.Terrain) > 0)
                    {
                        _mapService.MarkCellVisitedAndCheckForCompletion(party.CurrPos.MapId, nx, nz);
                    }
                }
            }

            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });
            await Task.CompletedTask;
            return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
        }
    }
}



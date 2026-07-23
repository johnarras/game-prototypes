using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using OxDb.SharedGame.Spells.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class PassWallSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.PassWall;


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {

            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            Point2I dir = DirUtils.AxisAngleToDirDelta((party.CurrPos.Rot + 90) % 360 / 90 * 90);

            if (dir == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Improper map data found." };
            }

            int nx = party.CurrPos.X + dir.X;
            int nz = party.CurrPos.Z + dir.Z;


            if (nx < 0 || nz < 0 || nx >= map.Width || nz >= map.Height)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That is out of bounds." };
            }

            if (map.Get(nx, nz, CellIndex.Terrain) == 0)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "The world does not exist there." };
            }

            _spellService.RemoveSpellPowerCost(party, action.Action.Member, action.Spell);

            _mapService.MovePartyTo(party, nx, nz, party.CurrPos.Rot, true, token);
            await Task.CompletedTask;
            return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
        }
    }
}



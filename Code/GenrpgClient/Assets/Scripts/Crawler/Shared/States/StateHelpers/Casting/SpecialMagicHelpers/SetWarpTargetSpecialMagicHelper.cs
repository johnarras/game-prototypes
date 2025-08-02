using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using System.Threading;
using System.Threading.Tasks;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class SetWarpTargetSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long Key => SpecialMagics.SetWarpTarget;


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap map = world.GetMap(party.CurrPos.MapId);

            action.Action.Member.WarpMapId = party.CurrPos.MapId;
            action.Action.Member.WarpMapX = party.CurrPos.X;
            action.Action.Member.WarpMapZ = party.CurrPos.Z;
            action.Action.Member.WarpRot = party.CurrPos.Rot;

            stateData.AddText("Warp terget set.");
            stateData.Actions.Add(new CrawlerStateAction("Press " + _textService.HighlightText("Space") + " to return to the map.", CharCodes.Space, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}

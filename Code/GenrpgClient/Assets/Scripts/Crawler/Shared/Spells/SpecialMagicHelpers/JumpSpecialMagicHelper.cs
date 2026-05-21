using Assets.Scripts.UI.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Spells.Settings.SpecialMagic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class JumpSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.Jump;


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {

            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap map = world.GetMap(party.CurrPos.MapId);

            if (!string.IsNullOrEmpty(action.PreviousError))
            {

                stateData.AddText(_textService.HighlightText(action.PreviousError, TextColors.ColorRed));
            }

            stateData.AddText("Jump from 1-9 squares");

            stateData.AddInputField("Jump: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnJump(party, action.Action.Member, map, action, text, token);
                }
            });

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.SelectSpell));

            await Task.CompletedTask;
            return stateData;
        }

        private void OnJump(PartyData party, PartyMember member, CrawlerMap map, SelectSpellAction action, string text,
            CancellationToken token)
        {
            if (!int.TryParse(text, out int distance) || distance < 1 || distance > 9)
            {
                CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true)
                {
                    ExtraData = action,
                };
                action.PreviousError = "The jump distance must be between 1 and 9 spaces.";
                _crawlerService.ChangeState(ECrawlerStates.SpecialSpellCast, token, action);

            }

            _logService.Info("Ok Jump Distance Input" + distance);


            PointXZ dir = DirUtils.AxisAngleToDirDelta((party.CurrPos.Rot + 90) % 360 / 90 * 90);

            if (dir == null)
            {
                _crawlerService.ChangeState(ECrawlerStates.Error, token, "Bad party direction");
                return;
            }
            _spellService.RemoveSpellPowerCost(party, member, action.Spell);

            int cx = party.CurrPos.X;
            int cz = party.CurrPos.Z;
            for (int i = 0; i < distance; i++)
            {
                if (_mapService.GetBlockingBits(map, cx, cz, cx + dir.X, cz + dir.Z, false) != WallTypes.None)
                {
                    _crawlerService.ChangeState(ECrawlerStates.Error, token, "Path is blocked");
                    return;
                }
                cx += dir.X;
                cz += dir.Z;
            }

            _mapService.MovePartyTo(party, cx, cz, party.CurrPos.Rot, true, token);
            _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
        }
    }
}



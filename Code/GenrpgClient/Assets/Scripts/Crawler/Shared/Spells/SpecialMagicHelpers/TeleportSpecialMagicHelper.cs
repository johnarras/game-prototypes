using OxDb.Client.UI.Constants;
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
    public class TeleportSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.Teleport;


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

            stateData.AddText("Teleport to any (X,Y) coordinate\nin the current map.");

            stateData.AddInputField("X: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnJump(party, action.Action.Member, map, action, text, stateData, token);
                }
            });

            stateData.AddInputField("Y: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnJump(party, action.Action.Member, map, action, text, stateData, token);
                }
            });

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.SelectSpell));

            await Task.CompletedTask;
            return stateData;
        }

        private void OnJump(PartyData party, PartyMember member, CrawlerMap map, SelectSpellAction action, string text,
            CrawlerStateData currState,
         CancellationToken token)
        {
            if (currState.Inputs.Count < 2)
            {
                _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
            }

            if (!int.TryParse(currState.Inputs[0].InputField.GetInputText(), out int x))
            {
                return;
            }

            if (!int.TryParse(currState.Inputs[1].InputField.GetInputText(), out int z))
            {
                return;
            }

            if (x < 0 || x >= map.Width || z < 0 || z >= map.Height)
            {
                CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true)
                {
                    ExtraData = action,
                };
                action.PreviousError = "Those coordinates are out of bounds.";
                _crawlerService.ChangeState(ECrawlerStates.SpecialSpellCast, token, action);
                return;

            }

            if (map.Get(x, z, CellIndex.Terrain) == 0)
            {
                CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true)
                {
                    ExtraData = action,
                };
                action.PreviousError = "That is not a valid target location.";
                _crawlerService.ChangeState(ECrawlerStates.SpecialSpellCast, token, action);
                return;

            }

            _spellService.RemoveSpellPowerCost(party, member, action.Spell);
            _mapService.MovePartyTo(party, x, z, party.CurrPos.Rot, true, token);
            _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
        }
    }
}



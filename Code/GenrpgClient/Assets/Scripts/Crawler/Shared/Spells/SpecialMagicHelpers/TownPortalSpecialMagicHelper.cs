using Assets.Scripts.UI.Constants;
using OxDb.SharedCore.Utils;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class TownPortalSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long HelperKey => SpecialMagics.TownPortal;

        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            List<CrawlerMap> cities = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.City).OrderBy(x => x.Level).ToList();

            foreach (CrawlerMap cityMap in cities)
            {
                if (!party.CompletedMaps.HasBitIndex(cityMap.IdKey))
                {
                    continue;
                }

                int ptx = cityMap.Width / 2;
                int ptz = cityMap.Height / 2;


                int newRot = (DirUtils.DirDeltaToAngle(ptx, ptz) + 270) % 360;

                EnterCrawlerMapData mapData = new EnterCrawlerMapData()
                {
                    MapId = cityMap.IdKey,
                    MapX = ptx,
                    MapZ = ptz,
                    MapRot = newRot,
                    World = world,
                    Map = cityMap,
                    IsTownPortal = true,
                };

                stateData.Actions.Add(new CrawlerStateAction(cityMap.Name + " (Level " + cityMap.Level + ")", Key.None, ECrawlerStates.ExploreWorld,
                   () =>
                   {
                       _spellService.RemoveSpellPowerCost(party, action.Action.Member, action.Spell);
                   },
                   mapData));
            }

            if (!string.IsNullOrEmpty(action.PreviousError))
            {

                stateData.AddText(_textService.HighlightText(action.PreviousError, TextColors.ColorRed));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.SelectSpell));

            await Task.CompletedTask;
            return stateData;
        }
    }
}



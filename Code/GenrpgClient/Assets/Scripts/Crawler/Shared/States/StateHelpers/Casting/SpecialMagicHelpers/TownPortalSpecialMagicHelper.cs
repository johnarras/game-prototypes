using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Assets.Scripts.UI.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class TownPortalSpecialMagicHelper : BaseSpecialMagicHelper
    {

        public override long Key => SpecialMagics.TownPortal;


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap overworld = world.GetMap(1);

            List<CrawlerMap> cities = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.City).OrderBy(x => x.Level).ToList();

            foreach (CrawlerMap cityMap in cities)
            {
                MapCellDetail entrance = overworld.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == cityMap.IdKey);

                if (entrance == null)
                {
                    continue;
                }

                if (!party.CompletedMaps.HasBit(cityMap.IdKey))
                {
                    continue;
                }

                int ptx = 0;
                int ptz = 0;

                if (entrance.ToX == 0)
                {
                    ptx = 1;
                }
                else if (entrance.ToX == cityMap.Width - 1)
                {
                    ptx = -1;
                }
                else if (entrance.ToZ == 0)
                {
                    ptz = 1;
                }
                else
                {
                    ptz = -1;
                }

                int newRot = (DirUtils.DirDeltaToAngle(ptx, ptz) + 270) % 360;

                EnterCrawlerMapData mapData = new EnterCrawlerMapData()
                {
                    MapId = entrance.EntityId,
                    MapX = entrance.ToX,
                    MapZ = entrance.ToZ,
                    MapRot = newRot,
                    World = world,
                    Map = cityMap,
                    IsTownPortal = true,
                };

                stateData.Actions.Add(new CrawlerStateAction(cityMap.Name + " (Level " + cityMap.Level +")", CharCodes.None, ECrawlerStates.ExploreWorld,
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

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.SelectSpell));

            await Task.CompletedTask;
            return stateData;
        }
    }
}

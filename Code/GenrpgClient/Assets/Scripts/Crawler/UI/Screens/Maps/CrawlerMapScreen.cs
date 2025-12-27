using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.Screens.Maps
{
    public class CrawlerMapScreen : TypedArgScreen<CrawlerMapScreenArgs>
    {

        private ICrawlerService _crawlerService = null;
        private ICrawlerWorldService _worldService = null;
        public CrawlerTilemap Tilemap;
        public GText MapName;

        private PartyData _party;
        private CrawlerWorld _world;
        private CrawlerMap _map;
        protected override async Task OnStartOpen(CrawlerMapScreenArgs mapArgs, CancellationToken token)
        {

            if (Tilemap == null)
            {
                StartClose();
                return;
            }
            _party = _crawlerService.GetParty();

            if (_party == null)
            {
                StartClose();
                return;
            }

            _world = await _worldService.GetWorld(_party.WorldId);

            if (_world == null)
            {
                StartClose();
                return;
            }


            if (mapArgs == null)
            {
                mapArgs = new CrawlerMapScreenArgs()
                {
                    MapId = _party.CurrPos.MapId
                };
            }

            _map = _worldService.GetMap(mapArgs.MapId);

            if (_map == null)
            {
                StartClose();
                return;
            }

            _uiService.SetText(MapName, _map.Name);

            CrawlerTilemapInitData initData = new CrawlerTilemapInitData()
            {
                Height = _map.Height,
                Width = _map.Width,
                MapId = _party.CurrPos.MapId,
                XOffset = 0,
                ZOffset = 0,
            };

            await Tilemap.Init(initData);

            await Task.CompletedTask;
        }
    }
}



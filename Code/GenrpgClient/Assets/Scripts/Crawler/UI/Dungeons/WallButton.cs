using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Props;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Dungeons;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.UI.Dungeons
{
    public class WallButton : CrawlerProp, IPointerClickHandler
    {
        public MeshRenderer MeshRenderer;
        private ICrawlerMapService _mapService = null;
        public override void InitData(int x, int z, CrawlerMap map)
        {
            base.InitData(x, z, map);
            CrawlerMapRoot root = _mapService.GetMapRoot();

            DungeonMaterials wallMats = root.GetMaterialsAt(x, z);

            if (wallMats != null)
            {

                MeshRenderer.sharedMaterial = wallMats.GetMaterials(DungeonAssetIndex.Walls)[0].Mat;
                float colorScale = 1.1f;
                MeshRenderer.material.color = new Color(colorScale, colorScale, colorScale, colorScale);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            CrawlerMap map = _mapService.GetMapRoot().Map;

            PartyData partyData = _crawlerService.GetParty();

            int index = map.GetEntityId(partyData.CurrPos.X, partyData.CurrPos.Z, EntityTypes.Riddle);

            if (index > 0)
            {
                partyData.AddRiddleBitIndex(index - 1);
                _logService.Info("Click Button index: " + index);
            }


            _mapService.ClearCellObject(partyData.CurrPos.X, partyData.CurrPos.Z);
        }
    }
}

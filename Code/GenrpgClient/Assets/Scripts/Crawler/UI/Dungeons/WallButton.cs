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
        public override void Init()
        {
            CrawlerMapRoot root = _mapService.GetMapRoot();

            MeshRenderer.material = root.DungeonMaterials.GetMaterials(DungeonAssetIndex.Walls)[0].Mat;
            float colorScale = 0.7f;
            MeshRenderer.material.color = new Color(colorScale, colorScale, colorScale, colorScale);
            base.Init();
        }


        public void OnPointerClick(PointerEventData eventData)
        {

            CrawlerMap map = _mapService.GetMapRoot().Map;

            PartyData partyData = _crawlerService.GetParty();

            int index = map.GetEntityId(partyData.CurrPos.X, partyData.CurrPos.Z, EntityTypes.Riddle);

            if (index > 0)
            {
                partyData.RiddleStatus |= (long)(1 << (index - 1));
                _logService.Info("Click Button index: " + index);
            }


            _mapService.ClearCellObject(partyData.CurrPos.X, partyData.CurrPos.Z);
        }
    }
}

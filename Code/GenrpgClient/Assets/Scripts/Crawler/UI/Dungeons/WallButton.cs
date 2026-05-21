using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Props;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Dungeons;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.UI.Dungeons
{
    public class WallButton : CrawlerProp, IPointerClickHandler
    {
        public MeshRenderer MeshRenderer;
        private ICrawlerMapService _mapService = null;
        public override void SetData(CrawlerObjectLoadData loadData)
        {
            base.SetData(loadData);
            CrawlerMapRoot root = _mapService.GetMapRoot();

            FinalDungeonMaterials wallMats = root.GetMaterialsAt(_cell.MapX, _cell.MapZ);

            if (wallMats != null)
            {
                MeshRenderer.sharedMaterial = wallMats.GetMaterials(DungeonPrefabIndexes.Walls)[0].Mat;
                float colorScale = 1.1f;
                MeshRenderer.material.color = new Color(colorScale, colorScale, colorScale, colorScale);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CrawlerMap map = _mapService.GetMapRoot().Map;

            PartyData party = _crawlerService.GetParty();

            int index = map.GetEntityId(party.CurrPos.X, party.CurrPos.Z, EntityTypes.Riddle);

            if (index > 0)
            {
                party.AddRiddleBitIndex(index - 1);
                _logService.Info("Click Button index: " + index);
            }

            _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
        }
    }
}



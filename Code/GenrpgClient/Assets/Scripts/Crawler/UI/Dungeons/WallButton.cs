using OxDb.Client.Assets.Textures;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Props;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Riddles.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OxDb.Client.Crawler.UI.Dungeons
{
    public class WallButton : MapProp, IPointerClickHandler
    {
        public EmissiveLerp EmissiveLerp;
        public MeshRenderer MeshRenderer;
        private ICrawlerMapService _mapService = null;
        private IRiddleService _riddleService = null;

        public override void SetData(CrawlerObjectLoadData loadData)
        {
            base.SetData(loadData);
            CrawlerMapRoot root = _mapService.GetMapRoot();

            _riddleService.SetPropPosition(gameObject, loadData, GetToken());


            MaterialBlock block = root.GetMaterialBlockAt(loadData.Cell.MapX, loadData.Cell.MapZ);

            if (block != null)
            {
                EmissiveLerp.SetColor(block.ForegroundColor);
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
            }

            _mapService.ClearCellProps(party.CurrPos.X, party.CurrPos.Z);
        }
    }
}



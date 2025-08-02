
using Assets.Scripts.Crawler.Maps.ClientEvents;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Newtonsoft.Json.Bson;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Props
{
    public class CrawlerProp : BaseBehaviour
    {
        protected IClientAppService _appService = null;
        protected ICrawlerService _crawlerService = null;

        public float RotateAnglePerSecond = 0;

        protected float _targetFrameRate = 30;

        public GameObject OffObject;
        public GameObject OnObject;

        protected int _x = -1;
        protected int _z = -1;

        protected MapCellDetail _detail = null;
        protected CrawlerMap _map = null;
        protected PartyData _party = null;
        protected CrawlerMapStatus _status = null;

        public override void Init()
        {

            _updateService.AddUpdate(gameObject, OnUpdate, UpdateTypes.Regular, GetToken());

            _dispatcher.AddListener<RedrawMapCell>(OnRedrawMapCell, GetToken());

            _targetFrameRate = _appService.TargetFrameRate;
        }

        public virtual void InitData(int x, int z, CrawlerMap map)
        { 
            _x = x;
            _z = z;
            _detail = map.Details.FirstOrDefault(d=>d.X ==x && d.Z == z);
            _map = map;
            _party = _crawlerService.GetParty();
            _status = _party.GetMapStatus(_map.IdKey, true);
        }

        protected virtual void OnUpdate()
        {

            if (RotateAnglePerSecond > 0)
            {
                float angleThisFrame = RotateAnglePerSecond * 1.0f / _targetFrameRate;

                transform.Rotate(0, angleThisFrame, 0);
            }
            
        }

        protected void OnRedrawMapCell(RedrawMapCell redrawCell)
        {
            if (redrawCell.X == _x && redrawCell.Z == _z)
            {
                OnRedrawMapCellInternal(redrawCell.Data);
            }
        }

        protected virtual void OnRedrawMapCellInternal (object obj)
        {

        }

    }
}

using Assets.Scripts.Crawler.Maps.ClientEvents;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Shared.GameEvents;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Props
{
    public class CrawlerProp : BaseBehaviour
    {
        protected IClientAppService _appService = null;
        protected ICrawlerService _crawlerService = null;
        protected ICameraController _cameraController = null;

        public float RotateAnglePerSecond = 0;

        protected float _targetFrameRate = 30;

        public GameObject OffObject;
        public GameObject OnObject;

        protected MapCellDetail _detail = null;
        protected CrawlerMapRoot _mapRoot = null;
        protected PartyData _party = null;
        protected CrawlerMapStatus _status = null;


        public AudioSource AmbientSound;

        protected ClientMapCell _cell = null;

        public override void Init()
        {

            AddUpdate(OnUpdate, UpdateTypes.Regular);

            _dispatcher.AddListener<RedrawMapCell>(OnRedrawMapCell, GetToken());

            _dispatcher.AddListener<MovePartyEvent>(OnMoveParty, GetToken());

            _targetFrameRate = _appService.TargetFrameRate;


            OnMoveParty(new MovePartyEvent());
        }

        public virtual void SetData(CrawlerObjectLoadData loadData)
        {
            _cell = loadData.Cell;
            _mapRoot = loadData.MapRoot;
            _detail = _mapRoot.Map.Details.FirstOrDefault(d => d.X == _cell.MapX && d.Z == _cell.MapZ);
            _party = _crawlerService.GetParty();
            _status = _party.GetMapStatus(_mapRoot.Map.IdKey, true);
        }

        protected virtual void OnUpdate()
        {

            if (RotateAnglePerSecond > 0)
            {
                float elapsedSeconds = _appService.TotalElapsedTime();
                float totalAngle = elapsedSeconds * RotateAnglePerSecond;
                transform.eulerAngles = new Vector3(0, totalAngle, 0);
            }

        }

        protected void OnRedrawMapCell(RedrawMapCell redrawCell)
        {
            if (redrawCell.X == _cell.MapX && redrawCell.Z == _cell.MapZ)
            {
                OnRedrawMapCellInternal(redrawCell.Data);
            }
        }

        protected virtual void OnRedrawMapCellInternal(object obj)
        {

        }

        protected virtual void OnMoveParty(MovePartyEvent onMove)
        {

            if (AmbientSound == null || _mapRoot == null || _mapRoot.AssetBlockList == null)
            {
                return;
            }

            Camera mainCam = _cameraController.GetMainCamera();

            double distance = Vector3.Distance(transform.position, mainCam.transform.position);

            bool nearby = distance < _mapRoot.AssetBlockList.BlockXZSize * 3;

            if (!nearby)
            {
                AmbientSound.Stop();
            }
            else
            {
                AmbientSound.loop = true;
                AmbientSound.Play();
            }
        }
    }
}



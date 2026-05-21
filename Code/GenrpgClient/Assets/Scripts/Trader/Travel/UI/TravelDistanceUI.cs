using Assets.Scripts.Assets.Sprites.Services;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Travel.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Cities.Settings;
using OxDb.SharedGame.Trader.Travel.Entities;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Trader.Travel.UI
{
    public class TravelDistanceUI : BaseBehaviour
    {
        private ICaravanService _caravanService = null;
        private ISpriteService _spriteService = null;

        public GameObject InCityAnchor;

        public GText CityNameText;

        public GameObject TravelAnchor;

        public GImage CaravanIcon;

        public GText TargetCityName;


        public RectTransform TravelBGRect;

        public RectTransform CaravanIconRect;

        private long _totalDistanceToTarget = 0;
        private long _targetDistanceGone = 0;
        private float _currDistanceGone = 0;

        private bool _lastWasInCity = false;


        private long _caravanSkinId = 0;

        public override void Init()
        {
            base.Init();

            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateTraderHUD, GetToken());
            _dispatcher.AddListener<TravelDay>(OnShowTravelDay, GetToken());

            _updateService.AddUpdate(this, UpdateCurrentDistance, UpdateTypes.Regular, GetToken());

            ShowStatus(true);
        }

        private void OnUpdateTraderHUD(UpdateTraderHUD updateUI)
        {
            if (updateUI.FullRefresh)
            {
                ShowStatus(true);
            }
        }

        private void OnShowTravelDay(TravelDay day)
        {
            ShowStatus(false);
            _targetDistanceGone = day.Vars[DayVars.EndDistance];
        }

        private void UpdateCurrentDistance()
        {
            UpdateCurrantDistanceInternal(false);
        }

        private void UpdateCurrantDistanceInternal(bool forceUpdate)
        {
            if (_totalDistanceToTarget < 1)
            {
                return;
            }

            _currDistanceGone = MathUtil.Clamp(0, _currDistanceGone, _totalDistanceToTarget);
            _targetDistanceGone = MathUtil.Clamp(0, _targetDistanceGone, _totalDistanceToTarget);
            if (!forceUpdate && _currDistanceGone == _targetDistanceGone)
            {
                return;
            }

            if (_currDistanceGone < _targetDistanceGone)
            {
                _currDistanceGone += 1.0f / ClientTravelService.FramesPerUnitOfDistance;
            }
            else if (_currDistanceGone < _targetDistanceGone)
            {
                _currDistanceGone += 1.0f / ClientTravelService.FramesPerUnitOfDistance;
            }


            float positionPercent = 0;

            if (_totalDistanceToTarget > 0)
            {
                positionPercent = 1.0f * _currDistanceGone / _totalDistanceToTarget;
            }
            _uiService.PlaceChildInParentRect(CaravanIconRect, TravelBGRect, positionPercent, 0.5f);
        }

        private long _lastCityIdNameShown = 0;
        private void ShowStatus(bool instant)
        {

            CoreData coreData = _gs.ch.Get<CoreData>();
            CaravanPosition pos = _caravanService.GetPosition(coreData);

            CaravanData caravanData = _gs.ch.Get<CaravanData>();

            _caravanSkinId = caravanData.SkinTypeId;

            SkinType skinType = _gameData.Get<SkinTypeSettings>(_gs.ch).Get(_caravanSkinId);

            if (skinType == null)
            {
                skinType = _gameData.Get<SkinTypeSettings>(_gs.ch).GetData().First();
            }

            _spriteService.SetEntityIcon(EntityTypes.SkinType, _caravanSkinId, CaravanIcon, base.GetToken());

            long currCityIdToShow = 0;

            if (pos.GetCurrentCity() != null)
            {
                _clientEntityService.SetActive(TravelAnchor, false);
                _clientEntityService.SetActive(InCityAnchor, true);
                _lastWasInCity = true;
                currCityIdToShow = pos.GetCurrentCity().IdKey;
            }
            else
            {
                _clientEntityService.SetActive(TravelAnchor, true);
                _clientEntityService.SetActive(InCityAnchor, false);
                if (_lastWasInCity)
                {
                    instant = true;
                    _lastWasInCity = false;
                }

                _targetDistanceGone = pos.DistanceGone;
                _totalDistanceToTarget = pos.TotalDistanceToTarget;
                if (instant)
                {
                    _currDistanceGone = pos.DistanceGone;
                    UpdateCurrantDistanceInternal(true);
                }

                if (pos.GetTargetCityId() > 0)
                {
                    currCityIdToShow = pos.GetTargetCityId();
                }
            }

            if (_lastCityIdNameShown != currCityIdToShow)
            {
                City city = _gameData.Get<CitySettings>(_gs.ch).Get(currCityIdToShow);

                if (city != null)
                {
                    _lastCityIdNameShown = currCityIdToShow;
                    _uiService.SetText(CityNameText, city.Name);
                    _uiService.SetText(TargetCityName, "-> " + city.Name);
                }
                else
                {
                    if (_lastCityIdNameShown != -1)
                    {
                        _lastCityIdNameShown = -1;
                        _uiService.SetText(CityNameText, "Wilderness");
                        _uiService.SetText(TargetCityName, "Wilderness");
                    }
                }
            }
        }
    }
}

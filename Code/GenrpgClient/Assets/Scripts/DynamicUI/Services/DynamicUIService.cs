using Assets.Scripts.Assets;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.Doobers.UI;
using Assets.Scripts.WorldCanvas.GameEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.DynamicUI.Services
{
    public interface IDynamicUIService : IInitializable
    {
    }



    public class DynamicUIService : IDynamicUIService
    {

        class DooberTarget
        {
            public GameObject Go { get; set; }
            public RectTransform Rt { get; set; }
        }


        const string Subdirectory = "DynamicUI";

        private DynamicUIScreen _dooberScreen;
        private IScreenService _screenService;
        private IDispatcher _dispatcher;
        private ILogService _logService;
        private IClientUpdateService _updateService;
        private IClientEntityService _clientEntityService;
        private IInputService _inputService;
        private ICameraController _cameraController;
        private IAssetService _assetService;

        private Dictionary<string, DooberTarget> _dooberTargets = new Dictionary<string, DooberTarget>();

        private List<DynamicUIItem> _currentItems = new List<DynamicUIItem>();
        private List<DynamicUIItem> _removeList = new List<DynamicUIItem>();

        private GameObject _worldSpaceAnchor = null;
        private GameObject _screenSpaceAnchor = null;

        private CancellationToken _token;

        private Camera _mainCam = null;

        private Doober _dooberPrefab;

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<DynamicUIItem>(OnDynamicUIItem, _token);
            _updateService.AddUpdate(this, OnUpdate, UpdateTypes.Regular, _token);
            _dispatcher.AddListener<SetDooberTarget>(OnSetDooberTarget, token);
            _dispatcher.AddListener<ShowDoober>(OnShowDoober, token);        
            _mainCam = _cameraController?.GetMainCamera() ?? null;
            await Task.CompletedTask;
        }

        private void OnSetDooberTarget(SetDooberTarget sdt)
        {
            string key = sdt.EntityTypeId + "." + sdt.EntityId;

            if (_dooberTargets.ContainsKey(key))
            {
                _dooberTargets.Remove(key);
            }

            RectTransform rt = sdt.Target.GetComponent<RectTransform>();

            DooberTarget dt = new DooberTarget()
            {
                Go = sdt.Target,
                Rt = rt,
            };

            _dooberTargets[key] = dt;

            _clientEntityService.RegisterDestroyCallback(sdt.Target, () =>
            {
                if (_dooberTargets.TryGetValue(key, out DooberTarget dt))
                {
                    if (dt.Go == sdt.Target)
                    {
                        _dooberTargets.Remove(key);
                    }
                }
            });
        }

        private Vector2 GetDooberTarget(IReward reward)
        {
            return GetDooberTarget(reward.EntityTypeId, reward.EntityId);
        }

        private Vector2 GetDooberTarget(long entityTypeId, long entityId)
        {
            if (_dooberScreen == null)
            {
                _dooberScreen = (DynamicUIScreen)_screenService.GetScreen(ScreenNames.DynamicUI).Screen;
            }

            string key = entityTypeId + "." + entityId;

            if (!_dooberTargets.TryGetValue(key, out DooberTarget dt))
            {
                _logService.Warning("No doober target for " + entityTypeId + " " + entityId);
                return Vector2.zero;
            }

            return dt.Rt.position;

        }

        private void SetupAnchors()
        {
            if (_worldSpaceAnchor == null || _screenSpaceAnchor == null)
            {
                DynamicUIScreen dynamicUI = (DynamicUIScreen)_screenService.GetScreen(ScreenNames.DynamicUI).Screen;

                _worldSpaceAnchor = dynamicUI.WorldSpaceAnchor;
                _screenSpaceAnchor = dynamicUI.ScreenSpaceAnchor;
                _dooberPrefab = dynamicUI.DooberPrefab;
                _token = CancellationTokenSource.CreateLinkedTokenSource(_token, dynamicUI.GetToken()).Token;
            }
        }

        private void OnShowDoober(ShowDoober showDoober)
        {

            SetupAnchors();

            Vector3 startPosition = showDoober.StartPosition;

            if (startPosition == Vector3.zero)
            {
                startPosition = _dooberScreen.ScreenSpaceAnchor.transform.position;
            }

            Vector2 endPos = Vector2.zero;

            if (showDoober.EndPosition == Vector3.zero)
            {
                endPos = GetDooberTarget(showDoober.EntityTypeId, showDoober.EntityId);
            }
            else
            {
                endPos = showDoober.EndPosition;
            }

            Vector2 startPos = RectTransformUtility.WorldToScreenPoint(_mainCam, startPosition);

                
            Doober doober = _clientEntityService.FullInstantiate(_dooberPrefab);
            _clientEntityService.AddToParent(doober, _screenSpaceAnchor);

            if (!string.IsNullOrEmpty(showDoober.AtlasName) && !string.IsNullOrEmpty(showDoober.SpriteName))
            {
                doober.InitData(showDoober.AtlasName, showDoober.SpriteName, startPos, endPos);
            }
            else
            {
                doober.InitData(showDoober.EntityTypeId, showDoober.EntityId, showDoober.Quantity, startPos, endPos);
            }

            DynamicUIItem dooberCanvasItem = new DynamicUIItem (doober.gameObject, doober, startPos, DynamicUILocation.ScreenSpace);

            _currentItems.Add(dooberCanvasItem);
        }

        private GameObject GetAnchor(DynamicUILocation loc)
        {
            if (loc == DynamicUILocation.ScreenSpace)
            {
                return _screenSpaceAnchor;
            }
            else
            {
                return _worldSpaceAnchor;
            }
        }

        public void OnDynamicUIItem (DynamicUIItem item)
        {
            if (item != null && item.Go != null && item.WCI != null)
            {
                SetupAnchors();
                _clientEntityService.AddToParent(item.Go,GetAnchor(item.Location));
                item.Go.transform.position = item.StartPos;
                _clientEntityService.RegisterDestroyCallback(item.Go, () =>
                {
                    _removeList.Add(item);
                });
                
                _currentItems.Add(item);
            }
        }

        private void ProcessRemoveItems()
        {
            List<DynamicUIItem> removeListCopy = new List<DynamicUIItem>(_removeList);
            _removeList.Clear();
            foreach (DynamicUIItem wci in removeListCopy)
            {
                if (_currentItems.Contains(wci))
                {
                    _currentItems.Remove(wci);
                }
                _clientEntityService.Destroy(wci.Go);
            }
        }

        protected void OnUpdate()
        {

            if (_inputService == null)
            {
                return;
            }
            float delta = _inputService.GetDeltaTime();

            ProcessRemoveItems();
            foreach (DynamicUIItem wci in _currentItems)
            {
                if (wci.WCI != null)
                {
                    if (wci.WCI.FrameUpdateIsComplete(delta))
                    {
                        _removeList.Add(wci);
                    }
                }
            }
        }
    }
}

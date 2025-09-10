using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.Doobers.UI;
using Assets.Scripts.UI.Interfaces;
using Assets.Scripts.WorldCanvas.GameEvents;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DynamicUI.Services
{
    public interface IDynamicUIService : IInitializable, IClientResetCleanup
    {
    }



    public class DynamicUIService : IDynamicUIService
    {

        class DooberTarget
        {
            public GameObject Go { get; set; }
            public RectTransform Rt { get; set; }
        }

        public const string Subdirectory = "DynamicUI";

        public const string DooberPrefabName = "Doober";

        private DynamicUIScreen _dooberScreen = null;
        private IScreenService _screenService = null;
        private IDispatcher _dispatcher = null;
        private ILogService _logService = null;
        private IClientUpdateService _updateService = null;
        private IClientEntityService _clientEntityService = null;
        private IInputService _inputService = null;
        private ICameraController _cameraController = null;

        private Dictionary<string, DooberTarget> _dooberTargets = new Dictionary<string, DooberTarget>();

        private List<DynamicUIItem> _currentItems = new List<DynamicUIItem>();
        private List<DynamicUIItem> _removeList = new List<DynamicUIItem>();

        private GameObject _worldSpaceAnchor = null;
        private GameObject _screenSpaceAnchor = null;

        private CancellationToken _token;

        private Camera _mainCam = null;


        private ObjectPool _assetPool = new ObjectPool();


        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<DynamicUIItem>(OnDynamicUIItem, _token);
            _dispatcher.AddListener<ShowDynamicUIItem>(OnShowDynamicUIItem, _token);
            _updateService.AddUpdate(this, OnUpdate, UpdateTypes.Regular, _token);
            _dispatcher.AddListener<SetDooberTarget>(OnSetDooberTarget, token);
            _dispatcher.AddListener<ShowDooberEvent>(OnShowDoober, token);
            _mainCam = _cameraController?.GetMainCamera() ?? null;

            await Task.CompletedTask;
        }

        public async Task OnClientResetCleanup(CancellationToken token)
        {
            await Task.CompletedTask;
            _assetPool.Clear();
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
                _token = CancellationTokenSource.CreateLinkedTokenSource(_token, dynamicUI.GetToken()).Token;
            }
        }

        private void OnShowDoober(ShowDooberEvent showDoober)
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

            Vector2 startPos = startPosition;

            if (!showDoober.StartsInUI)
            {
                startPos = RectTransformUtility.WorldToScreenPoint(_mainCam, startPosition);
            }

            Vector2 diff = new Vector2(startPos.x - startPosition.x, startPos.y - startPosition.y);

            showDoober.EndPosition = endPos;
            showDoober.StartPosition = startPos;

            ShowDynamicUIItem showItem = new ShowDynamicUIItem(DynamicUILocation.ScreenSpace,
               DooberPrefabName, startPos, OnLoadDoober, showDoober, _token, Subdirectory);

            OnShowDynamicUIItem(showItem);

        }

        private void OnShowDynamicUIItem(ShowDynamicUIItem showItem)
        {
            _assetPool.CheckoutObject(showItem, AssetCategoryNames.UI, showItem.AssetName,
                OnLoadDynamicItem, showItem, showItem.Token, showItem.Subdirectory);
        }


        private void OnLoadDynamicItem(object obj, object data, CancellationToken token)
        {
            ShowDynamicUIItem showItem = data as ShowDynamicUIItem;

            if (showItem == null || showItem.Handler == null)
            {
                return;
            }

            GameObject go = obj as GameObject;
            if (go == null)
            {
                return;
            }

            OnDynamicUIItem(new DynamicUIItem(go, _clientEntityService.GetInterface<IDynamicUIItem>(go), showItem.StartPos, DynamicUILocation.ScreenSpace, _assetPool));

            showItem.Handler(obj, showItem.Data, token);

        }

        private void OnLoadDoober(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                return;
            }

            ShowDooberEvent showDoober = data as ShowDooberEvent;

            if (data == null)
            {
                _clientEntityService.Destroy(go);
            }

            Doober doober = _clientEntityService.GetComponent<Doober>(go);

            if (doober == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            if (!string.IsNullOrEmpty(showDoober.AtlasName) && !string.IsNullOrEmpty(showDoober.SpriteName))
            {
                doober.InitData(showDoober.AtlasName, showDoober.SpriteName, showDoober);
            }
            else
            {
                doober.InitData(showDoober.EntityTypeId, showDoober.EntityId, showDoober.Quantity, showDoober);
            }

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

        public void OnDynamicUIItem(DynamicUIItem item)
        {
            if (item != null && item.Go != null && item.WCI != null)
            {
                SetupAnchors();
                _clientEntityService.AddToParent(item.Go, GetAnchor(item.Location));
                item.Go.transform.position = item.StartPos;

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

                if (wci.Pool != null)
                {
                    wci.Pool.ReturnObject(wci.WCI);
                }
                else
                {
                    _clientEntityService.Destroy(wci.Go);
                }
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

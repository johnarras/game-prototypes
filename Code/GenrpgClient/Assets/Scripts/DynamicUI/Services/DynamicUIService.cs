using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.Doobers.UI;
using Assets.Scripts.DynamicUI.Interfaces;
using Assets.Scripts.GameObjects;
using Assets.Scripts.WorldCanvas.GameEvents;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.DynamicUI.Services
{
    public interface IDynamicUIService : IInitializable, IClientResetCleanup
    {
        void AddEntityQuantityVisual(long entityTypeId, long entityId, long quantityAdded, bool instant);
        DooberArgs CheckoutDooberArgs(bool randomPath = true);

        void ReturnDooberArgs(DooberArgs dooberArgs);

        bool ShowDefaultEntityDoober(long entityTypeId, long entityId, long quantity);

        bool ShowEntityDooberWithStartPosition(long entityTypeId, long entityId, long quantity, bool startsInUI, Vector3 startPosition);

        DooberArgs CheckoutSimpleEntityDooberArgs(long entityTypeId, long entityId, long quantity);

        DooberArgs CheckoutEntityDooberArgs(long entityTypeId, long entityId, long quantity, bool startsInUI, Vector3 startPosition);

        bool ShowDoober(DooberArgs dooberArgs);
    }



    public class DynamicUIService : IDynamicUIService
    {

        class DooberTarget
        {
            public GameObject Go { get; set; }
            public RectTransform Rt { get; set; }

            public bool IsMainTarget { get; set; }

            public IEntityQuantityIcon EntityQuantityIcon { get; set; }
        }

        public const string Subdirectory = "DynamicUI";

        public const string DooberPrefabName = "Doober";

        private DynamicUIScreen _dynamicUIScreen = null;
        private IScreenService _screenService = null;
        private IDispatcher _dispatcher = null;
        private ILogService _logService = null;
        private IClientUpdateService _updateService = null;
        private IClientEntityService _clientEntityService = null;
        private IInputService _inputService = null;
        private ICameraController _cameraController = null;
        private IObjectPool _objectPool = null;
        private IRandom _rand = null;

        private Dictionary<string, List<DooberTarget>> _dooberTargets = new Dictionary<string, List<DooberTarget>>();

        private List<DynamicUIItem> _currentItems = new List<DynamicUIItem>();
        private List<DynamicUIItem> _removeList = new List<DynamicUIItem>();

        private GameObject _worldSpaceAnchor = null;
        private GameObject _screenSpaceAnchor = null;

        private CancellationToken _token;

        private Camera _mainCam = null;


        private ConcurrentQueue<DooberArgs> _dooberArgPool = new ConcurrentQueue<DooberArgs>();


        public DooberArgs CheckoutDooberArgs(bool randomPaths = true)
        {
            if (!_dooberArgPool.TryDequeue(out DooberArgs result))
            {
                result = new DooberArgs();
            }

            if (randomPaths)
            {
                result.PercentDonePowerMult = 0.5f;
                result.StartOffsetSize = MathUtil.FloatRange(250,500, _rand);
            }
            return result;
        }

        public void ReturnDooberArgs(DooberArgs dooberArgs)
        {
            dooberArgs.Clear();
            _dooberArgPool.Enqueue(dooberArgs);  
        }

        public async Task Initialize(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddListener<DynamicUIItem>(OnDynamicUIItem, _token);
            _dispatcher.AddListener<ShowDynamicUIItem>(OnShowDynamicUIItem, _token);
            _updateService.AddUpdate(this, OnUpdate, UpdateTypes.Regular, _token);
            _dispatcher.AddListener<SetDooberTarget>(OnSetDooberTarget, token);
            _mainCam = _cameraController?.GetMainCamera() ?? null;

            await Task.CompletedTask;
        }

        public async Task OnReset(CancellationToken token)
        {
            foreach (DynamicUIItem di in _currentItems)
            {
                _clientEntityService.Destroy(di.Go);
            }

            await Task.CompletedTask;
        }

        private string GetDooberTargetKey(long entityTypeId, long entityId)
        {
            return entityTypeId + "." + entityId;
        }

        private void OnSetDooberTarget(SetDooberTarget sdt)
        {
            string key = GetDooberTargetKey(sdt.EntityTypeId, sdt.EntityId);

            if (_dooberTargets.ContainsKey(key))
            {
                _dooberTargets.Remove(key);
            }

            RectTransform rt = sdt.Target.GetComponent<RectTransform>();

            DooberTarget dt = new DooberTarget()
            {
                Go = sdt.Target,
                Rt = rt,
                IsMainTarget = sdt.IsMainDooberTarget,
                EntityQuantityIcon = sdt.EntityQuantityIcon,
            };

            if (!_dooberTargets.ContainsKey(key))
            {
                _dooberTargets[key] = new List<DooberTarget>();
            }
            _dooberTargets[key].Add(dt);

            _clientEntityService.RegisterDestroyCallback(sdt.Target, () =>
            {
                if (_dooberTargets.TryGetValue(key, out List<DooberTarget> targets))
                {
                    targets.Remove(dt);
                }
            });
        }

        private Vector2 GetDooberTarget(long entityTypeId, long entityId)
        {
            if (_dynamicUIScreen == null)
            {
                _dynamicUIScreen = (DynamicUIScreen)_screenService.GetScreen(ScreenNames.DynamicUI).Screen;
            }

            string key = GetDooberTargetKey(entityTypeId, entityId);

            if (!_dooberTargets.TryGetValue(key, out List<DooberTarget> targets))
            {
                _logService.Warning("No doober target for " + entityTypeId + " " + entityId);
                return Vector2.zero;
            }

            DooberTarget mainDt = targets.FirstOrDefault(x => x.IsMainTarget);

            if (mainDt == null)
            {
                _logService.Warning("No doober target for " + entityTypeId + " " + entityId);
                return Vector2.zero;
            }

            return mainDt.Rt.position;

        }

        private void SetupAnchors()
        {
            if (_worldSpaceAnchor == null || _screenSpaceAnchor == null)
            {
                DynamicUIScreen dynamicUI = (DynamicUIScreen)_screenService.GetScreen(ScreenNames.DynamicUI).Screen;

                _dynamicUIScreen = dynamicUI;
                _worldSpaceAnchor = dynamicUI.WorldSpaceAnchor;
                _screenSpaceAnchor = dynamicUI.ScreenSpaceAnchor;
                _token = CancellationTokenSource.CreateLinkedTokenSource(_token, dynamicUI.GetToken()).Token;
            }
        }

        public bool ShowDoober(DooberArgs dooberArgs)
        {

            SetupAnchors();

            Vector3 startPosition = dooberArgs.StartPosition;

            if (startPosition == Vector3.zero)
            {
                startPosition = _dynamicUIScreen.ScreenSpaceAnchor.transform.position;
            }

            Vector2 endPos = Vector2.zero;

            if (dooberArgs.EndPosition == Vector3.zero)
            {
                endPos = GetDooberTarget(dooberArgs.EntityTypeId, dooberArgs.EntityId);
            }
            else
            {
                endPos = dooberArgs.EndPosition;
            }

            Vector2 startPos = startPosition;

            if (!dooberArgs.StartsInUI)
            {
                startPos = RectTransformUtility.WorldToScreenPoint(_mainCam, startPosition);
            }

            Vector2 diff = new Vector2(startPos.x - startPosition.x, startPos.y - startPosition.y);

            dooberArgs.EndPosition = endPos;
            dooberArgs.StartPosition = startPos;

            ShowDynamicUIItem showItem = new ShowDynamicUIItem(DynamicUILocation.ScreenSpace,
               DooberPrefabName, startPos, OnLoadDoober, dooberArgs, _token, Subdirectory);

            OnShowDynamicUIItem(showItem);

            return true;

        }

        private void OnShowDynamicUIItem(ShowDynamicUIItem showItem)
        {
            _objectPool.CheckoutObject(showItem, AssetCategoryNames.UI, showItem.AssetName,
                OnLoadDynamicItem, showItem, showItem.Token, showItem.Subdirectory);
        }


        private void OnLoadDynamicItem(GameObject go, ShowDynamicUIItem showItem, CancellationToken token)
        {
            if (showItem == null || showItem.Handler == null)
            {
                return;
            }

            OnDynamicUIItem(new DynamicUIItem(go, _clientEntityService.GetInterface<IDynamicUIItem>(go), showItem.StartPos, DynamicUILocation.ScreenSpace));

            showItem.Handler(go, showItem.Data, token);

        }

        private void OnLoadDoober(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                return;
            }

            DooberArgs dooberArgs = data as DooberArgs;

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

            if (!string.IsNullOrEmpty(dooberArgs.AtlasName) && !string.IsNullOrEmpty(dooberArgs.SpriteName))
            {
                doober.SetData(dooberArgs.AtlasName, dooberArgs.SpriteName, dooberArgs);
            }
            else
            {
                doober.SetData(dooberArgs.EntityTypeId, dooberArgs.EntityId, dooberArgs.Quantity, dooberArgs);
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
                _objectPool.ReturnObject(wci.WCI);
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

        public void AddEntityQuantityVisual(long entityTypeId, long entityId, long quantityAdded, bool instant)
        {

            string key = GetDooberTargetKey(entityTypeId, entityId);

            if (!_dooberTargets.TryGetValue(key, out List<DooberTarget> targets))
            {
                return;
            }

            foreach (DooberTarget target in targets)
            {
                if (target.EntityQuantityIcon != null)
                {
                    target.EntityQuantityIcon.AddVisualQuantity(entityTypeId, entityId, quantityAdded, instant);
                }
            }
        }

        public bool ShowDefaultEntityDoober(long entityTypeId, long entityId, long quantity)
        {
            return ShowEntityDooberWithStartPosition(entityTypeId, entityId, quantity, true, Vector3.zero);
        }

        public bool ShowEntityDooberWithStartPosition(long entityTypeId, long entityId, long quantity, bool startsInUI, Vector3 startPosition)
        {
            DooberArgs dooberArgs = CheckoutDooberArgs();

            dooberArgs.EntityTypeId = entityTypeId;   
            dooberArgs.EntityId = entityId;   
            dooberArgs.Quantity = quantity;
            dooberArgs.StartsInUI = startsInUI;
            dooberArgs.StartPosition = startPosition;

            return ShowDoober(dooberArgs);
        }

        public DooberArgs CheckoutSimpleEntityDooberArgs(long entityTypeId, long entityId, long quantity)
        {
            return CheckoutEntityDooberArgs(entityTypeId, entityId, quantity, true, Vector3.zero);
        }

        public DooberArgs CheckoutEntityDooberArgs(long entityTypeId, long entityId, long quantity, bool startsInUI, Vector3 startPosition)
        {
            DooberArgs dooberArgs = CheckoutDooberArgs();
            dooberArgs.EntityTypeId = entityTypeId; 
            dooberArgs.EntityId = entityId;
            dooberArgs.Quantity = quantity;
            dooberArgs.StartsInUI |= startsInUI;
            dooberArgs.StartPosition = startPosition;

            return dooberArgs;
        }
    }
}



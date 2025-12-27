using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.GameObjects;
using Assets.Scripts.UI.Entities;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IScreenService : IInitializable, IClientQuitCleanup
{
    ActiveScreen GetScreen(long screenId);
    ActiveScreen GetLayerScreen(long layerId);
    List<ActiveScreen> GetScreensNamed(long screenId);
    public ActiveScreen GetScreen(string screenName);
    List<ActiveScreen> GetAllScreens();

    object GetDragParent();
    string GetSubdirectory(long screenId);
    string GetFullScreenNameFromId(long id);
    long GetScreenIdFromName(string screenName);
    Task<IScreen> OpenAsync(long screenId, object data, CancellationToken token);
    bool IsInitialized();
}


public class ScreenService : IScreenService
{
    private IAnalyticsService _analyticsService = null;
    private IAwaitableService _awaitableService = null;
    private IClientUpdateService _updateService = null;
    private IAssetService _assetService = null;
    private IClientEntityService _clientEntityService = null;
    private IGameData _gameData = null;
    private IClientGameState _gs = null;
    private ISingletonContainer _singletonContainer = null;
    private ILogService _logService = null;
    private IClientAppService _appService = null;
    private IDispatcher _dispatcher = null;

    private List<ClientScreenLayer> _layers = new List<ClientScreenLayer>();

    public List<long> AllowMultiQueueScreens;


    private GameObject _screenRoot = null;

    public GameObject DragParent;

    private bool _quitting = false;

    private CancellationToken _token;
    public async Task Initialize(CancellationToken token)
    {
        _token = token;

        _awaitableService.ForgetAwaitable(SetupLayers());
        _updateService.AddUpdate(this, LateScreenUpdate, UpdateTypes.Late, _token);
        _dispatcher.AddListener<CloseAllScreens>(OnCloseAllScreens, _token);
        _dispatcher.AddListener<FinishCloseScreen>(OnFinishCloseScreen, _token);
        _dispatcher.AddListener<CloseScreen>(OnCloseScreen, _token);
        _dispatcher.AddListener<OpenScreen>(OnOpenScreen, _token);

        await Task.CompletedTask;
    }


    public bool IsInitialized()
    {
        return _isInitialized;
    }


    private bool _isInitialized = false;
    private async Awaitable SetupLayers()
    {
        if (_isInitialized || !_appService.IsPlaying)
        {
            return;
        }

        ScreenLayerSettings layerSettings = null;

        do
        {
            await Awaitable.NextFrameAsync(_token);
            layerSettings = _gameData.Get<ScreenLayerSettings>(null);
        }
        while (layerSettings == null);

        _screenRoot = _singletonContainer.GetAssetParent<ActiveScreen>();

        _clientEntityService.DestroyAllChildren(_screenRoot);

        IReadOnlyList<ScreenLayer> layers = layerSettings.GetData();

        _layers = new List<ClientScreenLayer>();

        foreach (ScreenLayer layer in layers)
        {
            ClientScreenLayer clientLayer = new ClientScreenLayer()
            {
                Layer = layer,
            };
            _layers.Add(clientLayer);

            clientLayer.LayerParent = new GameObject() { name = layer.Name + "Layer" };
            _clientEntityService.AddToParent(clientLayer.LayerParent, _screenRoot);

            if (layer.IdKey == ScreenLayers.DragItems)
            {
                DragParent = clientLayer.LayerParent;
                Canvas canvas = DragParent.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10000;
            }
        }

        _isInitialized = true;
    }

    public virtual object GetDragParent()
    {
        return DragParent;
    }

    private void LateScreenUpdate()
    {
        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen != null || layer.CurrentLoading != null)
            {
                continue;
            }
            if (layer.ScreenQueue == null || layer.ScreenQueue.Count < 1)
            {
                continue;
            }

            ActiveScreen nextItem = layer.ScreenQueue[0];
            layer.CurrentLoading = nextItem;
            layer.ScreenQueue.RemoveAt(0);

            string prefabName = GetFullScreenNameFromId(nextItem.ScreenId);
            string subdirectory = GetSubdirectory(nextItem.ScreenId);

            ScreenOverrideSettings overrideSettings = _gameData.Get<ScreenOverrideSettings>(_gs.ch);

            if (overrideSettings != null) // This will not exist during the very earliest screens, so check it.
            {
                ScreenOverride screenOverride = overrideSettings.GetData().FirstOrDefault(x => x.DefaultScreenNameId == nextItem.ScreenId);

                if (screenOverride != null)
                {
                    ScreenName overrideName = _gameData.Get<ScreenNameSettings>(_gs.ch).Get(screenOverride.ReplaceScreenNameId);

                    if (overrideName != null)
                    {
                        prefabName = GetFullScreenNameFromId(overrideName.IdKey);
                    }
                }
            }

            _assetService.LoadAssetInto(layer.LayerParent, AssetCategoryNames.UI,
                prefabName, OnLoadScreen, _token, nextItem, subdirectory);

        }
    }

    private void OnLoadScreen(GameObject go, ActiveScreen active, CancellationToken token)
    {
        _awaitableService.ForgetAwaitable(OnLoadScreenAsync(go, active, token));
    }

    private async Awaitable OnLoadScreenAsync(GameObject screen, ActiveScreen nextItem, CancellationToken token)
    {
        if (screen == null)
        {
            _logService.Debug("Couldn't load screen ");
            return;
        }

        if (nextItem == null)
        {
            _logService.Debug("Couldn't find active screen object for new screen");
            _clientEntityService.Destroy(screen);
            return;
        }

        ClientScreenLayer layer = nextItem.LayerObject as ClientScreenLayer;

        if (layer == null)
        {
            _logService.Debug("Couldn't find active screen layer for new screen");
            _clientEntityService.Destroy(screen);
            return;
        }


        BaseScreen bs = screen.GetComponent<BaseScreen>();

        if (bs == null)
        {
            _clientEntityService.Destroy(screen);
            _logService.Debug("Screen had no BaseScreen on it");
            return;
        }
        bs.ScreenId = nextItem.ScreenId;
        bs.Subdirectory = GetSubdirectory(bs.ScreenId);

        List<Canvas> allCanvases = _clientEntityService.GetComponents<Canvas>(bs.gameObject);

        if (allCanvases.Count > 0)
        {
            int minSortingOrder = allCanvases.Min(x => x.sortingOrder);
            foreach (Canvas c in allCanvases)
            {
                c.sortingOrder = (int)(layer.Layer.IdKey * 10 + (c.sortingOrder - minSortingOrder));
            }
        }

        nextItem.Screen = bs;

        _analyticsService.Send(AnalyticsEvents.OpenScreen, nextItem.Screen.GetName());
        List<Canvas> canvases = _clientEntityService.GetComponents<Canvas>(nextItem.Screen);


        _clientEntityService.SetActive(nextItem.Screen, false);

        foreach (Canvas canvas2 in canvases)
        {
            canvas2.enabled = false;
        }

        try
        {
            await nextItem.Screen.StartOpen(nextItem.Data, nextItem.Screen.GetToken());
        }
        catch (Exception ex)
        {
            _logService.Exception(ex, "ScreenStartOpen: " + nextItem.ScreenId);
        }
        ClearAllScreensList();

        await Awaitable.NextFrameAsync(token);

        _clientEntityService.SetActive(nextItem.Screen, true);
        foreach (Canvas canvas2 in canvases)
        {
            canvas2.enabled = true;
        }
        layer.CurrentScreen = nextItem;
        layer.CurrentLoading = null;
    }

    public void OnOpenScreen(OpenScreen open)
    {
        if (_quitting)
        {
            return;
        }

        ScreenName sname = _gameData.Get<ScreenNameSettings>(_gs.ch).Get(open.ScreenId);

        if (sname == null)
        {
            return;
        }

        ClientScreenLayer clientLayer = _layers.FirstOrDefault(x => x.Layer.IdKey == sname.ScreenLayerId);

        if (!sname.AllowMultiQueue)
        {
            List<ActiveScreen> currScreen = GetScreensNamed(open.ScreenId);

            if (currScreen != null && currScreen.Count > 0)
            {
                return;
            }

            foreach (ActiveScreen screen in clientLayer.ScreenQueue)
            {
                if (screen.ScreenId == open.ScreenId)
                {
                    return;
                }
            }
            if (clientLayer.CurrentLoading != null && clientLayer.CurrentLoading.ScreenId == open.ScreenId)
            {
                return;
            }
        }

        ActiveScreen act = new ActiveScreen();
        act.Data = open.Data;
        act.Screen = null;
        act.ScreenLayerId = clientLayer.Layer.IdKey;
        act.ScreenId = open.ScreenId;
        act.LayerObject = clientLayer;

        clientLayer.ScreenQueue.Add(act);
    }

    public string GetSubdirectory(long screenId)
    {
        return _gameData.Get<ScreenNameSettings>(_gs.ch).Get(screenId)?.Subdirectory ?? "";
    }

    public void OnCloseScreen(CloseScreen close)
    {
        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == close.ScreenId)
            {

                BaseScreen baseScreen = layer.CurrentScreen.Screen as BaseScreen;
                if (baseScreen != null)
                {
                    baseScreen.StartClose();
                }
                else
                {
                    layer.CurrentScreen = null;
                }
                break;
            }
        }
    }

    public void OnFinishCloseScreen(FinishCloseScreen finish)
    {
        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == finish.ScreenId)
            {

                BaseScreen baseScreen = layer.CurrentScreen.Screen as BaseScreen;
                if (baseScreen != null)
                {
                    _clientEntityService.Destroy(baseScreen.gameObject);
                }
                _analyticsService.Send(AnalyticsEvents.CloseScreen, baseScreen.GetName());
                layer.CurrentScreen = null;
                ClearAllScreensList();
                break;
            }
        }

    }

    public ActiveScreen GetLayerScreen(long layerId)
    {
        ClientScreenLayer layer = _layers.FirstOrDefault(x => x.Layer.IdKey == layerId);

        return layer?.CurrentScreen ?? null;
    }

    public ActiveScreen GetScreen(long screenId)
    {
        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen == null)
            {
                continue;
            }
            if (layer.CurrentScreen.ScreenId != screenId)
            {
                continue;
            }
            return layer.CurrentScreen;
        }
        return null;
    }

    public List<ActiveScreen> GetScreensNamed(long screenId)
    {
        List<ActiveScreen> retval = new List<ActiveScreen>();

        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen == null)
            {
                continue;
            }
            if (layer.CurrentScreen.ScreenId == screenId)
            {
                retval.Add(layer.CurrentScreen);
            }
        }
        return retval;
    }

    protected void ClearAllScreensList()
    {
        _allScreens = null;
    }

    private List<ActiveScreen> _allScreens = null;
    public List<ActiveScreen> GetAllScreens()
    {
        _allScreens = new List<ActiveScreen>();

        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen == null || layer.Layer.SkipInAllScreensList)
            {
                continue;
            }
            _allScreens.Add(layer.CurrentScreen);
        }
        return _allScreens;
    }


    private void OnCloseAllScreens(CloseAllScreens closeAll)
    {
        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen == null || layer.Layer.SkipInAllScreensList)
            {
                continue;
            }

            if (closeAll.KeepOpenScreens.Contains(layer.CurrentScreen.ScreenId))
            {
                continue;
            }

            OnCloseScreen(new CloseScreen(layer.CurrentScreen.ScreenId));
        }
    }

    public ActiveScreen GetScreen(string screenName)
    {
        string shortScreenName = screenName.Replace("Screen", "");


        IReadOnlyList<ScreenName> screenNames = _gameData.Get<ScreenNameSettings>(_gs.ch).GetData();

        ScreenName sname = screenNames.FirstOrDefault(x => x.Name == shortScreenName);

        if (sname == null)
        {
            return null;
        }

        foreach (ClientScreenLayer layer in _layers)
        {
            if (layer.CurrentScreen == null)
            {
                continue;
            }


            if (layer.CurrentScreen.ScreenId != sname.IdKey)
            {
                continue;
            }
            return layer.CurrentScreen;
        }
        return null;
    }

    public async Task<IScreen> OpenAsync(long screenId, object data, CancellationToken token)
    {
        await Awaitable.MainThreadAsync();
        OnOpenScreen(new OpenScreen(screenId, data));

        while (true)
        {
            ActiveScreen screen = GetScreen(screenId);
            if (screen != null && screen.Screen != null)
            {
                return screen.Screen;
            }
            await Awaitable.NextFrameAsync(token);
        }
    }

    public void OnQuit()
    {
        _quitting = true;
    }

    public string GetFullScreenNameFromId(long screenId)
    {

        ScreenName screenName = _gameData.Get<ScreenNameSettings>(_gs.ch).Get(screenId);
        return (screenName.Name.Replace("_", "/") + "Screen");
    }
    public long GetScreenIdFromName(string screenName)
    {
        string shortScreenName = screenName.Replace("Screen", "");

        ScreenName sname = _gameData.Get<ScreenNameSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name == shortScreenName);

        return sname?.IdKey ?? 0;

    }
}


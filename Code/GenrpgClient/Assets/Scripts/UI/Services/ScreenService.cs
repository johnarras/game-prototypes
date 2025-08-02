using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.UI.Config;
using System.Linq;
using System.Threading;
using Genrpg.Shared.UI.Settings;
using Genrpg.Shared.Analytics.Services;
using System.Threading.Tasks;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Client.Assets.Constants;
using Assets.Scripts.Awaitables;
using System;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.UI.Entities;
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.GameSettings.Settings;




public class ScreenService : BaseBehaviour, IScreenService, IGameTokenService, IInjectOnLoad<IScreenService>
{
    private IAnalyticsService _analyticsService;
    protected IAwaitableService _awaitableService;
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public List<ScreenLayer> Layers;

    public List<long> AllowMultiQueueScreens;

    public GameObject DragParent;

    private List<ScreenConfig> _screenConfigs = null;

    private CancellationToken _gameToken;
    public void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    public override void Init()
    {
        base.Init();
        SetupLayers();
        StartUpdates();
        _screenConfigs = _assetService.LoadAllResources<ScreenConfig>("ScreenConfigs");
    }

    public void StartUpdates()
    {
        AddUpdate(LateScreenUpdate, UpdateTypes.Late);
    }

    private bool _haveSetupLayers = false;
    private void SetupLayers()
    {
        if (_haveSetupLayers || Layers == null)
        {
            return;
        }
        _haveSetupLayers = true;
        _clientEntityService.DestroyAllChildren(entity);
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].CurrentScreen = null;
            Layers[i].ScreenQueue = new List<ActiveScreen>();
        }

        _clientEntityService.DestroyAllChildren(entity);
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].LayerParent = new GameObject();
            Layers[i].LayerParent.name = Layers[i].LayerId + "Layer";
            Layers[i].Index = i;
            _clientEntityService.AddToParent(Layers[i].LayerParent, entity);
            if (Layers[i].LayerId == ScreenLayers.DragItems)
            {
                DragParent = Layers[i].LayerParent;
                Canvas canvas = DragParent.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 10000;
            }
        }
    }
    public string GetFullScreenNameFromId(long screenId)
    {

        ScreenName screenName = _gameData.Get<ScreenNameSettings>(_gs.ch).Get(screenId);
        return (screenName.Name.Replace("_", "/") + "Screen");
    }

    public virtual object GetDragParent()
    {
        return DragParent;
    }

    private void LateScreenUpdate()
    {
        foreach (ScreenLayer layer in Layers)
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
                prefabName, OnLoadScreen, nextItem, _gameToken, subdirectory);
            
        }
    }

    private void OnLoadScreen(object obj, object data, CancellationToken token)
    {
        _awaitableService.ForgetAwaitable(OnLoadScreenAsync(obj, data, token));
    }

    private async Awaitable OnLoadScreenAsync (object obj, object data, CancellationToken token)
    { 
        GameObject screen = obj as GameObject;
        ActiveScreen nextItem = data as ActiveScreen;
        
        
        if (screen == null)
        {
            _logService.Debug("Couldn't load screen ");
            return;
        }

        if (nextItem ==null)
        {
            _logService.Debug("Couldn't find active screen object for new screen");
            _clientEntityService.Destroy(screen);
            return;
        }

        ScreenLayer layer = nextItem.LayerObject as ScreenLayer;

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
                c.sortingOrder = layer.Index * 10 + (c.sortingOrder - minSortingOrder);
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

    public void StringOpen (string screenName, object data = null)
    {

        ScreenName screenNameObj = _gameData.Get<ScreenNameSettings>(_gs.ch).Get(screenName);

        if (screenNameObj == null)
        {
            return;
        }

        ScreenConfig config = _screenConfigs.FirstOrDefault(x => x.EntityId == screenNameObj.IdKey);

        if (config != null)
        {
            Open(config.EntityId, data);
        }
    }

    public void Open(long screenId, object data = null)
    {
        ScreenLayer currLayer = GetLayer(screenId);
        if (currLayer == null)
        {
            _logService.Debug("Couldn't find layer for the screen " + screenId);
            return;
        }

        bool allowMultiScreens = false;
        if (AllowMultiQueueScreens != null)
        {
            allowMultiScreens = AllowMultiQueueScreens.Contains(screenId);
        }
        if (!allowMultiScreens)
        {

            List<ActiveScreen> currScreen = GetScreensNamed(screenId);

            if (currScreen != null && currScreen.Count > 0)
            {
                return;
            };
            foreach (ActiveScreen screen in currLayer.ScreenQueue)
            {
                if (screen.ScreenId == screenId)
                {
                    return;
                }
            }
            if (currLayer.CurrentLoading != null && currLayer.CurrentLoading.ScreenId == screenId)
            {
                return;
            }
        }

        ActiveScreen act = new ActiveScreen();
        act.Data = data;
        act.Screen = null;
        act.LayerId = currLayer.LayerId;
        act.ScreenId = screenId;
        act.LayerObject = currLayer;

        currLayer.ScreenQueue.Add(act);
    }

    public string GetSubdirectory(long screenId)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => x.EntityId== screenId);
        return config?.Subdirectory ?? null;
    }

    public ScreenLayer GetLayer(long screenId)
    {
        ScreenConfig config = _screenConfigs.FirstOrDefault(x => x.EntityId == screenId);

        ScreenLayers layerId = config?.ScreenLayer ?? ScreenLayers.Screens;

        return Layers.FirstOrDefault(x => x.LayerId == layerId);
    }


    public void Close(long screenId)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == screenId)
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

    public void FinishClose(long screenId)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen != null && layer.CurrentScreen.ScreenId == screenId)
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

    public ActiveScreen GetLayerScreen(ScreenLayers layerId)
    {
        ScreenLayer layer = Layers.FirstOrDefault(x => x.LayerId == layerId);

        return layer?.CurrentScreen ?? null;
    }

    public ActiveScreen GetScreen(long screenId)
    {
        foreach (ScreenLayer layer in Layers)
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

    public List<ActiveScreen> GetScreensNamed (long screenId)
    {
        List<ActiveScreen> retval = new List<ActiveScreen>();

        foreach (ScreenLayer layer in Layers)
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

        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null || layer.SkipInAllScreensList)
            {
                continue;
            }
            _allScreens.Add(layer.CurrentScreen);
        }
        return _allScreens;
    }


    public void CloseAll(List<long> ignoreScreens = null)
    {
        foreach (ScreenLayer layer in Layers)
        {
            if (layer.CurrentScreen == null || layer.SkipInAllScreensList)
            {
                continue;
            }

            if (ignoreScreens != null && ignoreScreens.Contains(layer.CurrentScreen.ScreenId))
            {
                continue;
            }

            Close(layer.CurrentScreen.ScreenId);
        }
    }

    public ActiveScreen GetScreen(string screenName)
    {
        string shortScreenName = screenName.Replace("Screen", "");


        IReadOnlyList<ScreenName> screenNames = _gameData.Get<ScreenNameSettings>(_gs.ch).GetData();

        ScreenName sname = screenNames.FirstOrDefault(x=>x.Name == shortScreenName); 

        if (sname == null)
        {
            return null;
        }

        foreach (ScreenLayer layer in Layers)
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
        Open(screenId, data);

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
}
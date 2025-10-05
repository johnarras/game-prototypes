using Assets.Scripts.Assets;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Threading;
using UnityEngine;

public class BaseBehaviour : StubComponent, IInitOnResolve, IExplicitInject
{
    protected IInitClient _initClient = null;
    protected IClientUpdateService _updateService = null;
    protected IScreenService _screenService = null;
    protected IRealtimeNetworkService _networkService = null;
    protected IAssetService _assetService = null;
    protected IUIService _uiService = null;
    protected ILogService _logService = null;
    protected IDispatcher _dispatcher = null;
    protected IGameData _gameData = null;
    protected IClientGameState _gs = null;
    protected IClientRandom _rand = null;
    protected IClientEntityService _clientEntityService = null;

    private CancellationTokenSource _cts = null;
    public CancellationToken GetToken()
    {
        if (_cts == null)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(_initClient.GetGameToken(), this.destroyCancellationToken);
        }
        return _cts.Token;
    }

    protected void ClearToken()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

    }

    public virtual void Init()
    {

    }
    public GameObject entity
    {
        get
        {
            return this.gameObject;
        }
    }

    protected void AddUpdate(Action func, int index)
    {
        _updateService?.AddUpdate(this, func, index, GetToken());
    }

    protected void AddTokenUpdate(Action<CancellationToken> func, int index)
    {
        _updateService?.AddTokenUpdate(this, func, index, GetToken());
    }

    protected void AddDelayedUpdate(Action<CancellationToken> func, float delaySeconds)
    {
        _updateService?.AddDelayedUpdate(this, func, delaySeconds, GetToken());
    }

    protected void AddListener<T>(GameAction<T> action) where T : class
    {
        _dispatcher.AddListener<T>(action, GetToken());
    }

    protected virtual void OnDestroy()
    {
        ClearToken();
    }
}


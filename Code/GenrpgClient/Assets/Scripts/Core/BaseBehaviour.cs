using OxDb.Client;
using OxDb.Client.Assets.ObjectPools;
using OxDb.Client.GameObjects;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using System;
using System.Threading;
using UnityEngine;

public abstract class BaseBehaviour : StubComponent, IInitOnResolve, IExplicitInject, IDestroyCallback, IPooledObject
{
    protected IClientUpdateService _updateService = null;
    protected IAssetService _assetService = null;
    protected IUIService _uiService = null;
    protected ILogService _logService = null;
    protected IDispatcher _dispatcher = null;
    protected IGameData _gameData = null;
    protected IClientGameState _gs = null;
    protected IClientEntityService _clientEntityService = null;
    private IInitClient _initClient = null;

    private CancellationTokenSource _cts = null;
    public virtual CancellationToken GetToken()
    {
        if (_cts == null)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(_initClient.GetGameToken(), this.destroyCancellationToken);
        }
        return _cts.Token;
    }

    protected CancellationTokenRegistration _ctRegistration;

    public void SetDestroyCallback(Action action)
    {
        _ctRegistration.Dispose();

        if (action == null)
        {
            return;
        }

        _ctRegistration = destroyCancellationToken.Register(action);
    }

    protected void ClearToken()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        SetDestroyCallback(null);

    }

    public virtual string GetName()
    {
        return name;
    }

    public virtual void Init()
    {

    }
    public GameObject entity
    {
        get
        {
            return gameObject;
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

    protected void AddListener<T>(GameAction<T> action) where T : class, IClientEvent
    {
        _dispatcher.AddListener<T>(action, GetToken());
    }

    protected virtual void OnDestroy()
    {
        ClearToken();
    }

    public virtual void OnReturn()
    {

    }
}




using Assets.Scripts.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Threading;
using UnityEngine;

public abstract class BaseClientMapMessageHandler<T> : IClientMapMessageHandler where T : class, IMapApiMessage
{
    public Type HelperKey => typeof(T);

    protected IClientMapObjectManager _objectManager;
    protected IMapTerrainManager _terrainManager;
    protected IRepositoryService _repoService = null;
    protected ILogService _logService = null;
    protected IDispatcher _dispatcher;
    protected CancellationToken _token;
    protected IClientGameState _gs;
    protected IClientRandom _rand;

    protected abstract Awaitable InnerProcess(T msg, CancellationToken token);

    public async Awaitable Process(IMapApiMessage msg, CancellationToken token)
    {
        await InnerProcess(msg as T, token);
    }
}




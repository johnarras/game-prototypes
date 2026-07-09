using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.MapMessages.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    protected abstract ValueTask InnerProcess(T msg, CancellationToken token);

    public async ValueTask Process(IMapApiMessage msg, CancellationToken token)
    {
        await InnerProcess(msg as T, token);
    }
}




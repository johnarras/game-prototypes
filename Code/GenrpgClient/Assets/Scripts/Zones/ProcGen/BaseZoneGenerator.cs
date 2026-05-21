using Assets.Scripts.Awaitables;
using Assets.Scripts.Core;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Repository;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Pathfinding.Services;
using OxDb.SharedGame.ProcGen.Services;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BaseZoneGenerator : IZoneGenerator, IInitializable
{
    protected IAssetService _assetService = null;
    protected IFileDownloadService _fileDownloadService = null;
    protected IClientMapObjectManager _objectManager;
    protected IMapTerrainManager _terrainManager;
    protected INoiseService _noiseService = null;
    protected IZoneGenService _zoneGenService = null;
    protected CancellationToken _token;
    protected ILogService _logService = null;
    protected IClientRepositoryService _clientRepoService = null;
    protected IDispatcher _dispatcher;
    protected IGameData _gameData;
    protected IMapProvider _mapProvider;
    protected IClientGameState _gs;
    protected IClientRandom _rand;
    protected IMapGenData _md;
    protected IPathfindingService _pathfindingService = null;
    protected IClientEntityService _clientEntityService = null;
    protected IAwaitableService _awaitableService = null;

    public virtual async Awaitable Generate(CancellationToken token)
    {
        _token = token;
        await Task.CompletedTask;

    }

    public virtual async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }
}


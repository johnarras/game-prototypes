using Assets.Scripts.Awaitables;
using Assets.Scripts.GameSettings.Entities;
using Assets.Scripts.Logalytics.Services;
using Assets.Scripts.Options.Services;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.GameAuth.Interfaces;
using OxDb.SharedGame.MapServer.Entities;
using System.Collections.Generic;
using System.Reflection;



public interface IClientGameState : IGameState, IInjectable, IExplicitInject, IRandomContainer
{
    string GameUserId { get; set; }
    string ClientSessionId { get; set; }
    int SessionSequenceId { get; set; }
    IGameSessionState SessionState { get; set; }
    Character ch { get; set; }
    List<CharacterStub> characterStubs { get; set; }
    List<MapStub> mapStubs { get; set; }
    EGameModes GameMode { get; set; }
}

public class StubSessionState : IGameSessionState
{
    public string SelfContainedToken { get; set; }
    public string RefreshToken { get; set; }
    public string SessionId { get; set; }
    public string ServerName { get; set; }
    public string ServerVersion { get; set; }
    public string ServerEnv { get; set; }
}

public class ClientGameState : GameState, IInjectable, IClientGameState
{
    public IMapGenData md { get; set; } = null;
    public string GameUserId { get; set; }
    public string ClientSessionId { get; set; } = HashUtils.NewGuid();
    public int SessionSequenceId { get; set; }
    public IGameSessionState SessionState { get; set; } = new StubSessionState();
    public Character ch { get; set; }
    public List<CharacterStub> characterStubs { get; set; } = new List<CharacterStub>();
    public List<MapStub> mapStubs { get; set; } = new List<MapStub>();

    public EGameModes GameMode { get; set; }

    public string Version { get; set; }
    public string RealtimeHost { get; set; }
    public string RealtimePort { get; set; }

    public IRandom Rand { get; set; } = new MyRandom();

    private ILogService _logService = null;
    private IClientAppService _clientAppService = null;
    protected IAwaitableService _awaitableService = null;
    private ITextSerializer _serializer = null;
    public ClientGameState(ClientConfig config, IInitClient initClient)
    {
        _logService = new ClientLogService(config);
        ClientConfigContainer configContainer = new ClientConfigContainer(config);
        _clientAppService = new ClientAppService(_logService);

        IReflectionService reflectionService = new ReflectionService();
        reflectionService.AddSearchAssembly(Assembly.GetExecutingAssembly());

        _loc = new ServiceLocator(_logService, reflectionService, new ClientGameData());
        loc.Set(initClient);
        loc.Set(_clientAppService);
        loc.Set<IClientGameState>(this);
        loc.Set<IClientConfigContainer>(configContainer);
        loc.Set<IClientOptionsService>(new ClientOptionsService(_logService, _clientAppService, _serializer));
    }
}



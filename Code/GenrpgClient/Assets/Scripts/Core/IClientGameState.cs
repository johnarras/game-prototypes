using OxDb.Client;
using OxDb.Client.Awaitables;
using OxDb.Client.GameSettings.Entities;
using OxDb.Client.Logalytics.Services;
using OxDb.Client.Options.Services;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.GameAuth.Interfaces;
using OxDb.SharedGame.MapServer.Entities;
using System;
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
    public string FullToken { get; set; }
    public string RefreshToken { get; set; }
    public string GameSessionId { get; set; }
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

        _logService.Verbose("Reflection 0");
        IReflectionService reflectionService = new ReflectionService(_logService);
        _logService.Verbose("Reflection1: " + Assembly.GetExecutingAssembly().GetName());
        reflectionService.AddSearchAssembly(GetType().Assembly);

        _logService.Verbose("Reflection2: All: " + reflectionService.GetAllAssemblies().Length + " Search: " + reflectionService.GetSearchAssemblies(GetType().Assembly).Count);

        Rand = new MyRandom(DateTime.UtcNow.Ticks);

        _loc = new ServiceLocator(_logService, reflectionService, new ClientGameData());
        loc.Set(initClient);
        loc.Set(_clientAppService);
        loc.Set<IClientGameState>(this);
        loc.Set<IClientConfigContainer>(configContainer);
        loc.Set<IClientOptionsService>(new ClientOptionsService(_logService, _clientAppService, _serializer));
    }
}



using Assets.Scripts.Awaitables;
using Assets.Scripts.GameSettings.Entities;
using Assets.Scripts.Options.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Serialization.Interfaces;
using System.Collections.Generic;



public interface IClientGameState : IGameState, IInjectable, IExplicitInject
{
    string GameUserId { get; set; }
    string SessionId { get; set; }
    Character ch { get; set; }
    List<CharacterStub> characterStubs { get; set; }
    List<MapStub> mapStubs { get; set; }
    EGameModes GameMode { get; set; }
}

public class ClientGameState : GameState, IInjectable, IClientGameState
{
    public IMapGenData md { get; set; } = null;
    public string GameUserId { get; set; }
    public string SessionId { get; set; }
    public Character ch { get; set; }
    public List<CharacterStub> characterStubs { get; set; } = new List<CharacterStub>();
    public List<MapStub> mapStubs { get; set; } = new List<MapStub>();

    public EGameModes GameMode { get; set; }

    public string Version { get; set; }
    public string RealtimeHost { get; set; }
    public string RealtimePort { get; set; }

    private ILogService _logService = null;
    private IClientAppService _clientAppService = null;
    protected IAwaitableService _awaitableService = null;
    private ITextSerializer _serializer = null;
    public ClientGameState(ClientConfig config, IInitClient initClient)
    {
        ClientConfigContainer configContainer = new ClientConfigContainer(config);
        _logService = new ClientLogService(configContainer.Config);
        _clientAppService = new ClientAppService(_logService);
        _loc = new ServiceLocator(_logService, new ClientGameData());
        loc.Set(initClient);
        loc.Set(_clientAppService);
        loc.Set<IClientGameState>(this);
        loc.Set<IClientConfigContainer>(configContainer);
        loc.Set<IClientOptionsService>(new ClientOptionsService(_logService, _clientAppService, _serializer));
    }
}



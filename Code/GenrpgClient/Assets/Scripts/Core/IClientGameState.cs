using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;

using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.GameSettings.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Client.Core;
using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Core.Constants;
using UnityEngine;
using Genrpg.Shared.Utils;
using Assets.Scripts.Repository;



public interface IClientGameState : IGameState, IInjectable, IExplicitInject
{
    User user { get; set; }
    Character ch { get; set; }
    List<CharacterStub> characterStubs { get; set; }
    List<MapStub> mapStubs { get; set; }
    InitialClientConfig GetConfig();
    EGameModes GameMode { get; set; }
    void UpdateUserFlags(int flag, bool val);
}

public class ClientGameState : GameState, IInjectable, IClientGameState
{
    public IMapGenData md { get; set; } = null;   
    public User user { get; set; }
    public Character ch { get; set; }
    public List<CharacterStub> characterStubs { get; set; }  = new List<CharacterStub>();
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
        ClientConfigContainer configContainer = new ClientConfigContainer();
        configContainer.Config = config;
        ITextSerializer serializer = new NewtonsoftTextSerializer();
        _logService = new ClientLogService(configContainer.Config, serializer);
        IAnalyticsService analyticsService = new ClientAnalyticsService(configContainer.Config, _logService, serializer);
        loc = new ServiceLocator(serializer, _logService, analyticsService, new ClientGameData());
        loc.Set(initClient);   
        loc.Set<IClientGameState>(this);
        loc.Set<IClientConfigContainer>(configContainer);
    }

    protected string ConfigFilename = "InitialClientConfig";
    protected InitialClientConfig _config = null;
    public InitialClientConfig GetConfig()
    {
        if (_config == null)
        {
            ClientRepositoryCollection<InitialClientConfig> repo = new ClientRepositoryCollection<InitialClientConfig>(_logService, _clientAppService, _serializer);
            _config = repo.Load(ConfigFilename).GetAwaiter().GetResult();
            if (_config == null)
            {
                _config = new InitialClientConfig()
                {
                    Id = ConfigFilename,
                };
                // Do this here rather than in constructor because protobuf will ignore zeroes
                _config.UserFlags |= UserFlags.SoundActive | UserFlags.MusicActive;
                SaveConfig();
            }
        }
        return _config;
    }

    public void SaveConfig()
    {
        if (_config == null)
        {
            _config = new InitialClientConfig()
            {
                Id = ConfigFilename,
            };
        }
        _awaitableService.ForgetAwaitable(SaveConfig(_config));
    }

    private async Awaitable SaveConfig(InitialClientConfig config)
    {
        ClientRepositoryCollection<InitialClientConfig> repo = new ClientRepositoryCollection<InitialClientConfig>(_logService, _clientAppService, _serializer);
        await repo.Save(config);
    }

    public void UpdateUserFlags(int flag, bool val)
    {
        if (user == null)
        {
            return;
        }
        if (val)
        {
            user.AddFlags(flag);
        }
        else
        {
            user.RemoveFlags(flag);
        }

        InitialClientConfig config = GetConfig();
        config.UserFlags = user.Flags;
        SaveConfig();
    }

}

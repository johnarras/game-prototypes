
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.ComponentModel;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Setup;
using Genrpg.ServerShared.Core;
using Genrpg.MapServer.Networking;
using Genrpg.Shared.Crafting.PlayerData;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.CloudMessaging.Interfaces;
using Genrpg.Shared.Utils;
using System.Net;
using Newtonsoft.Json.Serialization;
using Genrpg.MapServer.Networking.Listeners;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Pings.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings.Messages;
using Genrpg.ServerShared.MainServer;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.Characters.Utils;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.Serialization.Serializers;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.ServerShared.Config;
using Genrpg.MapServer.Setup.Instances;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using ZstdSharp.Unsafe;
using Genrpg.Shared.Tasks.Services;

namespace Genrpg.MapServer.Maps
{
    public class MapInstance : BaseServer<ServerGameState,MapInstanceSetupService,IMapInstanceCloudMessageHandler>, IDisposable
    {
        private IListener _listener = null;

        private List<ServerConnectionState> _players = new List<ServerConnectionState>();

        private object _playersLock = new object();

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IStatService _statService = null;
        private IPathfindingService _pathfindingService = null;
        private IGameDataService _gameDataService = null;
        private IPlayerDataService _playerDataService = null;
        private IMapSpawnDataService _mapSpawnDataService = null;
        private IMapDataService _mapDataService = null;
        protected IRepositoryService _repoService = null;
        protected ILogService _logService = null;
        protected IMapProvider _mapProvider;
        private ITextSerializer _textSerializer = null;
        private IBinarySerializer _binarySerializer = null;
        private ITaskService _taskService = null;       

        public const double UpdateMS = 100.0f;

        private string _mapId;
        private string _instanceId;

        private string _host = null;
        private int _port = 0;
        private int _mapSize = 0;
        private EMapApiSerializers _serializerType;

        

        private IRandom _rand = new MyRandom();

        private CancellationTokenSource _instanceTokenSource;

        public MapInstance()
        {

        }

        public async Task Shutdown(int msDelay = 0)
        {
            _cloudCommsService.SendQueueMessage(CloudServerNames.Instance, new RemoveMapInstance() { FullInstanceId = _serverId });
            _instanceTokenSource?.CancelAfter(msDelay);
            await Task.CompletedTask;
        }

        public string GetMapId()
        {
            return _mapId;
        }

        public string GetInstanceId()
        {
            return _instanceId;
        }

        protected bool _isRunning = false;
        public bool IsRunning()
        {
            return _isRunning;
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
        }

        protected virtual IListener GetListener(string host, int port, EMapApiSerializers serializerType)
        {
            ISerializer serializer = (serializerType == EMapApiSerializers.MessagePack ? _binarySerializer : _textSerializer);

            return new BaseTcpListener(host, port, _logService, serializer, _taskService, AddConnection, ReceiveCommands, _tokenSource.Token);
        }

        public void RefreshGameData(IGameData gameData)
        {
            _taskService.ForgetTask(RefreshGameDataAsync(gameData), false);
        }

        private async Task RefreshGameDataAsync(IGameData gameData)
        {
            await _gameDataService.ReloadGameData();
            _messageService.UpdateGameData(gameData);
            UpdatePlayerClientData();
        }

        protected override async Task PreInit(object data, object parentObject, CancellationToken serverToken)
        {
            InitMapInstanceData initData = data as InitMapInstanceData;
            _mapId = initData.MapId;
            _instanceId = HashUtils.NewUUId();
            _serverId = GetServerId(null);
            await Task.CompletedTask;
        }

        protected override async Task FinalInit(object data, object parentObject, CancellationToken parentToken)
        {
            await base.FinalInit(data, parentObject, parentToken);

            InitMapInstanceData initData = data as InitMapInstanceData;
            _isRunning = true;
            
            _instanceTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken, _tokenSource.Token);

            // Step 2: Load map before setting up messaging and object manager
            _mapProvider.SetMap(await _mapDataService.LoadMap(_rand, _mapId));
            _mapProvider.SetSpawns(await _mapSpawnDataService.LoadMapSpawnData(_repoService, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion));

            // Step 3: Setup messaging and object systems
            _messageService.Init(_tokenSource.Token);
            _objectManager.Init(_rand, _tokenSource.Token);
            _port = initData.Port;
            _serializerType = initData.SerializerType;
            _host = "127.0.0.1";
            _mapSize = _mapProvider.GetMap().BlockCount;
            
            if (_config.DefaultEnv.IndexOf(EnvNames.Test) >= 0 || _config.DefaultEnv.IndexOf(EnvNames.Prod) >= 0)
            {
                _host = _config.PublicIP;
            }
            // Step 4: Setup listener
            _listener = GetListener(_host, initData.Port, initData.SerializerType);


            SendAddInstanceMessage();

            _taskService.ForgetTask(ProcessMap(_tokenSource.Token), true);

            await _pathfindingService.LoadPathfinding(
                _config.ContentRoot + "/" + Game.Prefix.ToLower() +
                _config.DataEnvs[EDataCategories.Worlds.ToString()] + "/");
        }

        public void SendAddInstanceMessage()
        {
            AddMapInstance addInstance = new AddMapInstance()
            {
                ServerId = _serverId,
                MapId = _mapId,
                InstanceId = _serverId,
                Port = _port,
                Host = _host,
                Size = _mapSize,
                SerializerType = _serializerType,
            };

            _cloudCommsService.SendQueueMessage(CloudServerNames.Instance, addInstance);
        }

        public void AddConnection(ServerConnectionState connState)
        {
            lock (_playersLock)
            {
                _players.Add(connState);
            }
        }        

        public void ReceiveCommands(List<IMapApiMessage> commands, CancellationToken token, object connStateObject )
        {
            ServerConnectionState connState = connStateObject as ServerConnectionState;

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (connState.ch == null)
            {
                foreach (IMapApiMessage obj in commands)
                {
                    if (obj is AddPlayer add)
                    {
                        _taskService.ForgetTask(AddPlayerHandler(connState, add), true);
                    }
                }
                return;
            }
            foreach (IMapApiMessage obj in commands)
            {
                if (obj is Ping || !(obj is IPlayerCommand))
                {
                    continue;
                }

                // Handle this here directly so that the sequencing stays the same.
                if (obj is InfrequentMessageEnvelope encoded)
                {
                    if (encoded.InfrequentApiMessage is IPlayerCommand pcomm)
                    {
                        _messageService.SendMessage(connState.ch, pcomm);
                    }
                }
                else
                {
                    _messageService.SendMessage(connState.ch, obj);
                }
            }
        }

        private async Task ProcessMap(CancellationToken token)
        {

            try
            {
                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(UpdateMS)))
                {
                    while (true)
                    {
                        UpdatePlayerConnections();
                        await timer.WaitForNextTickAsync(token).ConfigureAwait(false);
                    }
                }            
            }
            catch (OperationCanceledException ce)
            {
                _logService.Info("Shutdown MapInstance.ProcessPlayerConnections " + ce.Message);
            }
        }

        private void UpdatePlayerConnections()
        {
            List<ServerConnectionState> removePlayers = new List<ServerConnectionState>();
            lock (_playersLock)
            {
                removePlayers = _players.Where(x => x.conn.RemoveMe()).ToList();
                foreach (ServerConnectionState connState in removePlayers)
                {
                    _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new PlayerLeaveMap() { Id = connState.ch?.Id });
                }
                _players = _players.Where(x => !x.conn.RemoveMe()).ToList();
            }
            foreach (ServerConnectionState playerConn in removePlayers)
            {
                ShutdownConnection(playerConn);
            }
        }

        private void ShutdownConnection(ServerConnectionState connState)
        {
            if (!connState.conn.RemoveMe())
            {
                connState.conn.Shutdown(null, "ShutdownConnMapManager");
            }

            if (connState.ch == null)
            {
                return;
            }
            _playerDataService.SavePlayerData(connState.ch, true);
            Character ch = connState.ch;
            if (ch != null)
            {
                _objectManager.RemoveObject(_rand, ch.Id);
            }
            connState.ch = null;
        }

        public async Task AddPlayerHandler(ServerConnectionState connState, AddPlayer add)
        {
            try
            {
                MyRandom loadRand = new MyRandom();
                if (connState.ch != null)
                {
                    connState.conn.AddMessage(new ErrorMessage("Player already loaded"));
                    return;
                }

                User user = await _repoService.Load<User>(add.UserId);

                if (user == null)
                {
                    connState.conn.AddMessage(new ErrorMessage("User does not exist"));
                    return;
                }
                if (user.SessionId != add.SessionId)
                {
                    connState.conn.AddMessage(new ErrorMessage("Invalid session id"));
                    return;
                }
                bool didLoad = false;
                if (!_objectManager.GetObject(add.CharacterId, out MapObject mapObj))
                {
                    CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(add.CharacterId);

                    if (coreCh == null)
                    {
                        connState.conn.AddMessage(new ErrorMessage("Character does not exist"));
                        return;
                    }
                    Character ch = new Character(coreCh);
                    ch.SetConn(connState.conn);

                    if (ch.MapId != _mapId)
                    {
                        ch.X = _mapProvider.GetMap().SpawnX;
                        ch.Z = _mapProvider.GetMap().SpawnY;
                        ch.MapId = _mapId;
                    }

                    ch.NearbyGridsSeen = new List<PointXZ>();
                    connState.ch = ch;
                    List<IUnitData> allUnitData = await _playerDataService.LoadAllPlayerData(loadRand, user, ch);
                    foreach (IUnitData unitData in allUnitData)
                    {
                        ch.Set(unitData);
                    }
                    _gameDataService.SetGameDataOverrides(ch, true);
                    MapObjectGridItem gridItem = _objectManager.AddObject(loadRand, ch, null);

                    didLoad = true;
                }
                else
                {
                    Character ch = mapObj as Character;
                    ch.SetConn(connState.conn);
                    connState.ch = ch;
                    ch.NearbyGridsSeen = new List<PointXZ>();
                }

                if (connState.ch == null)
                {
                    connState.conn.ForceClose();
                    return;
                }

                _statService.CalcStats(connState.ch, true);
                if (didLoad)
                {
                    _messageService.SendMessage(connState.ch, connState.ch.GetCachedMessage<Regen>(true));
                    _messageService.SendMessage(connState.ch, connState.ch.GetCachedMessage<SaveDirty>(true));
                    SendPlayerEnterMapMessage(connState.ch);
                }
               
                connState.conn.AddMessage(new OnFinishLoadPlayer());
            }
            catch (Exception e)
            {
               _logService.Exception(e, "AddPlayer");
            }
        }

        protected void SendPlayerEnterMapMessage(Character ch)
        {
            PlayerEnterMap playerEnterMessage = new PlayerEnterMap()
            {
                Id = ch.Id,
                Name = ch.Name,
                Level = ch.Level,
                MapId = _mapId,
                InstanceId = _serverId,
                UserId = ch.UserId,
            };
            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, playerEnterMessage);
        }

        public void SendAllPlayerEnterMapMessages()
        {
            List<Character> characters = _objectManager.GetAllCharacters();
            foreach (Character ch in characters)
            {
                SendPlayerEnterMapMessage(ch);
            }
        }

        private void UpdatePlayerClientData()
        {
            _messageService.SendMessageToAllPlayers(new UpdateGameSettings());

            List<Character> allCharacters = _objectManager.GetAllCharacters();

            foreach (Character ch in allCharacters)
            {
                _gameDataService.SetGameDataOverrides(ch, true);
            }
        }

        protected override string GetServerId(object data)
        {
            return "minst" + _mapId + "." + _instanceId;
        }
    }
}

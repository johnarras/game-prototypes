using OxDb.MapServer.CloudMessaging.Interfaces;
using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.MapServer.Networking.Listeners;
using OxDb.MapServer.Setup.Instances;
using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;
using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Core;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.ServerCore.MainServer;
using OxDb.ServerGame.Maps;
using OxDb.ServerGame.MapSpawns;
using OxDb.ServerGame.PlayerData.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.GameSettings.Messages;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.Networking.Constants;
using OxDb.SharedGame.Networking.Entities;
using OxDb.SharedGame.Pathfinding.Services;
using OxDb.SharedGame.Pings.Messages;
using OxDb.SharedGame.Players.Messages;
using OxDb.SharedGame.Stats.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.MapServer.Maps
{
    public class MapInstance : BaseServer<ServerGameState, MapInstanceSetupService, IMapInstanceCloudMessageHandler>, IDisposable
    {
        private IListener _listener = null;

        private List<ServerConnectionState> _players = new List<ServerConnectionState>();

        private object _playersLock = new object();

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IStatService _statService = null;
        private IPathfindingService _pathfindingService = null;
        private IServerGameDataService _gameDataService = null;
        private IPlayerDataService _playerDataService = null;
        private IMapSpawnDataService _mapSpawnDataService = null;
        private IMapDataService _mapDataService = null;
        protected IFullRepositoryService _repoService = null;
        protected ILogService _logService = null;
        protected IMapProvider _mapProvider;
        private ITextSerializer _textSerializer = null;
        private IBinarySerializer _binarySerializer = null;
        private ITaskService _taskService = null;

        public const double UpdateMS = 100.0f;

        private string _mapId;

        private string _host = null;
        private int _port = 0;
        private int _mapSize = 0;
        private EMapApiSerializers _serializerType;



        private IRandom _rand = new MyRandom();

        private CancellationTokenSource _instanceTokenSource;

        public MapInstance()
        {

        }

        protected override bool UseInstanceId => true;
        protected override string GetBaseServerName() { return ServerNames.MapInstance; }


        public async Task Shutdown(int msDelay = 0)
        {
            _cloudCommsService.SendQueueMessage(ServerNames.InstanceManager, new RemoveMapInstance() { FullInstanceId = _serverId });
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
            _currServerToken.Cancel();
        }

        protected virtual IListener GetListener(string host, int port, EMapApiSerializers serializerType)
        {
            ISerializer serializer = (serializerType == EMapApiSerializers.MessagePack ? _binarySerializer : _textSerializer);

            return new BaseTcpListener(host, port, _logService, serializer, _taskService, AddConnection, ReceiveCommands, _currServerToken.Token);
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

        protected override async Task PreInit(ServerInitArgs args)
        {
            InitMapInstanceData initData = args.Data as InitMapInstanceData;
            _mapId = initData.MapId;
            _serverId = GetFullServerName(null);
            await Task.CompletedTask;
        }

        protected override async Task PostInit(ServerInitArgs args)
        {
            await base.PostInit(args);

            InitMapInstanceData initData = args.Data as InitMapInstanceData;
            _isRunning = true;

            _instanceTokenSource = CancellationTokenSource.CreateLinkedTokenSource(args.Token, _currServerToken.Token);

            // Step 2: Load map before setting up messaging and object manager
            _mapProvider.SetMap(await _mapDataService.LoadMap(_rand, _mapId));
            _mapProvider.SetSpawns(await _mapSpawnDataService.LoadMapSpawnData(_repoService, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion));

            // Step 3: Setup messaging and object systems
            _messageService.Init(_currServerToken.Token);
            _objectManager.Init(_rand, _currServerToken.Token);
            _port = initData.Port;
            _serializerType = initData.SerializerType;
            _host = "127.0.0.1";
            _mapSize = _mapProvider.GetMap().BlockCount;

            if (_config.Env != EnvNames.Local)
            {
                _host = _config.GetConfigVal(AppConfigKeys.PublicIP);
            }
            // Step 4: Setup listener
            _listener = GetListener(_host, initData.Port, initData.SerializerType);


            SendAddInstanceMessage();

            _taskService.ForgetTask(ProcessMap(_currServerToken.Token), true);

            await _pathfindingService.LoadPathfinding(
                _config.GetConfigVal(AppConfigKeys.ContentRoot) + "/" + _config.GetConfigVal(AppConfigKeys.ProductName) +
                _config.DataEnvs[EDataCategories.Worlds.ToString()] + "/");
        }

        public void SendAddInstanceMessage()
        {
            AddMapInstance addInstance = new AddMapInstance()
            {
                ServerName = _serverId,
                MapId = _mapId,
                InstanceId = _serverId,
                Port = _port,
                Host = _host,
                Size = _mapSize,
                SerializerType = _serializerType,
            };

            _cloudCommsService.SendQueueMessage(ServerNames.InstanceManager, addInstance);
        }

        public void AddConnection(ServerConnectionState connState)
        {
            lock (_playersLock)
            {
                _players.Add(connState);
            }
        }

        public void ReceiveCommands(List<IMapApiMessage> commands, CancellationToken token, object connStateObject)
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
                    _cloudCommsService.SendQueueMessage(ServerNames.Player, new PlayerLeaveMap() { Id = connState.ch?.Id });
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
            _playerDataService.SavePlayerData(connState.ch);
            Character ch = connState.ch;
            if (ch != null)
            {
                _objectManager.RemoveObject(_rand, ch.Id);
            }
            connState.ch = null;
        }

        public async Task AddPlayerHandler(ServerConnectionState connState, AddPlayer add)
        {
            MyRandom loadRand = new MyRandom();
            if (connState.ch != null)
            {
                connState.conn.SendError("Player already loaded");
                return;
            }

            GameAccount gameAcct = await _repoService.Load<GameAccount>(add.GameUserId);

            if (gameAcct == null)
            {
                connState.conn.SendError("User does not exist");
                return;
            }
            if (gameAcct.SessionId != add.SessionId)
            {
                connState.conn.SendError("Invalid session token");
                return;
            }
            bool didLoad = false;
            if (!_objectManager.GetObject(add.CharacterId, out MapObject mapObj))
            {
                CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(add.CharacterId);

                if (coreCh == null)
                {
                    connState.conn.SendError("Character does not exist");
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
                List<IUnitData> allUnitData = await _playerDataService.LoadAllPlayerData(loadRand, gameAcct.Id, new List<IUnitData>(), ch);
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
            _cloudCommsService.SendQueueMessage(ServerNames.Player, playerEnterMessage);
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
    }
}



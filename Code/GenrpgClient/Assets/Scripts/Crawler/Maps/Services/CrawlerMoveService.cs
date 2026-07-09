using Assets.Scripts.Audio.ClientEvents;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.MoveHelpers;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Dungeons;
using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Options.Services;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{

    public interface ICrawlerMoveService : IInitializable
    {
        Task AddMovementKeyInput(Key keyChar, CancellationToken token);
        void ClearMovement();
        void FinishMove(CrawlerMoveStatus status);
        bool UpdatingMovement();
        Task OnEnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token);
        Awaitable Move(CrawlerMoveStatus status, int forward, int right, CancellationToken token);
        Task Rot(CrawlerMoveStatus status, int delta, bool fastRotate, CancellationToken token);
        LastMoveStatus GetLastMoveStatus();
        void SetFullRot(float endRot);
        IReadOnlyList<MovementKeyCode> GetMovementKeyCodes(bool setupMovementCodesNow);

    }

    public class LastMoveStatus
    {
        public DateTime LastMoveTime;
        public int MovesSinceLastCombat;
    }

    public class MovementKeyNames
    {
        public const string Forward = "Forward";
        public const string TurnLeft = "TurnLeft";
        public const string TurnRight = "TurnRight";
        public const string Backward = "Backward";
        public const string StrafeLeft = "StrafeLeft";
        public const string StrafeRight = "StrafeRight";
    }

    public class MovementKeyCode
    {
        public Key Key { get; private set; }
        public int RotationAmount { get; private set; }
        public int ForwardAmount { get; private set; }
        public int RightAmount { get; private set; }
        public string Name { get; private set; }

        public MovementKeyCode(Key key, string name, int rotationAmount, int forwardAmount, int rightAmount)
        {
            Key = key;
            Name = name;
            RotationAmount = rotationAmount;
            ForwardAmount = forwardAmount;
            RightAmount = rightAmount;
        }
    }
    public class CrawlerMoveService : ICrawlerMoveService
    {
        OrderedSetupDictionaryContainer<Type, ICrawlerMoveHelper> _moveHelpers = new OrderedSetupDictionaryContainer<Type, ICrawlerMoveHelper>();

        private List<MovementKeyCode> _movementKeyCodes = new List<MovementKeyCode>();

        private ICrawlerService _crawlerService = null;
        private IDispatcher _dispatcher = null;
        private ILogService _logService = null;
        private ICrawlerWorldService _worldService = null;
        private IAwaitableService _awaitableService = null;
        private IPartyService _partyService = null;
        private ICrawlerMapService _mapService = null;
        private IClientAppService _appService = null;
        private IClientOptionsService _clientOptionsService = null;

        private CancellationToken _token;

        const float _movesPerSecond = 4.0f;
        const float _turnsPerSecond = 6.0f;

        private PartyData _party = null;
        private CrawlerWorld _world = null;
        private bool _updatingMovement = false;
        const int maxQueuedMoves = 2;
        Queue<Key> _movementQueue = new Queue<Key>();


        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            SetupMovementKeyCodes();

            _awaitableService.ForgetAwaitable(UpdateMovementInternal(_token));
            await Task.CompletedTask;
        }

        public void SetupMovementKeyCodes()
        {

            if (!_clientOptionsService.GetOptions().HasFlag(ClientFlags.ClassicMovement))
            {
                _movementKeyCodes = new List<MovementKeyCode>
                {
                    new MovementKeyCode(Key.W, MovementKeyNames.Forward, 0, 1, 0),
                    new MovementKeyCode(Key.UpArrow, MovementKeyNames.Forward, 0, 1, 0),

                    new MovementKeyCode(Key.S, MovementKeyNames.Backward, 2, 0, 0),
                    new MovementKeyCode(Key.DownArrow, MovementKeyNames.Backward,2, 0, 0),

                    new MovementKeyCode(Key.A, MovementKeyNames.TurnLeft, -1, 0, 0),
                    new MovementKeyCode(Key.LeftArrow, MovementKeyNames.TurnLeft, -1, 0, 0),

                    new MovementKeyCode(Key.D, MovementKeyNames.TurnRight, 1, 0, 0),
                    new MovementKeyCode(Key.RightArrow, MovementKeyNames.TurnRight, 1, 0, 0),


                    new MovementKeyCode(Key.Q, MovementKeyNames.StrafeLeft, 0, 0, -1),
                    new MovementKeyCode(Key.E, MovementKeyNames.StrafeRight, 0, 0, 1),
                };
            }
            else // WASDQE classic
            {

                _movementKeyCodes = new List<MovementKeyCode>
                {
                    new MovementKeyCode(Key.W, MovementKeyNames.Forward, 0, 1, 0),
                    new MovementKeyCode(Key.UpArrow, MovementKeyNames.Forward, 0, 1, 0),

                    new MovementKeyCode(Key.S, MovementKeyNames.Backward, 2, 0, 0),
                    new MovementKeyCode(Key.DownArrow, MovementKeyNames.Backward,2, 0, 0),

                    new MovementKeyCode(Key.Q, MovementKeyNames.TurnLeft, -1, 0, 0),
                    new MovementKeyCode(Key.LeftArrow, MovementKeyNames.TurnLeft, -1, 0, 0),

                    new MovementKeyCode(Key.E, MovementKeyNames.TurnRight, 1, 0, 0),
                    new MovementKeyCode(Key.RightArrow, MovementKeyNames.TurnRight, 1, 0, 0),


                    new MovementKeyCode(Key.A, MovementKeyNames.StrafeLeft, 0, 0, -1),
                    new MovementKeyCode(Key.D, MovementKeyNames.StrafeRight, 0, 0, 1),
                };
            }
        }

        public IReadOnlyList<MovementKeyCode> GetMovementKeyCodes(bool setupNow)
        {
            if (setupNow || _movementKeyCodes.Count < 1)
            {
                SetupMovementKeyCodes();
            }

            return _movementKeyCodes;
        }

        public async Task OnEnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token)
        {
            _movementQueue.Clear();
            _party = party;
            _world = await _worldService.GetWorld(_party.WorldId);
            _partyService.OnEnterMap(_party);
        }

        public bool UpdatingMovement()
        {
            return _updatingMovement;
        }

        public void FinishMove(CrawlerMoveStatus status)
        {
            if (status.MovedPosition)
            {
                _lastMoveStatus.MovesSinceLastCombat++;
            }
            _lastMoveStatus.LastMoveTime = DateTime.UtcNow;
        }


        public void ClearMovement()
        {
            _movementQueue.Clear();
            _updatingMovement = false;
        }

        public void SetUpdatingMovement(bool updatingMovement)
        {
            _updatingMovement = updatingMovement;
        }

        public async Task AddMovementKeyInput(Key keyChar, CancellationToken token)
        {
            if (_movementQueue.Count < maxQueuedMoves)
            {
                if (_movementKeyCodes.FastAny(x => x.Key == keyChar))
                {
                    _movementQueue.Enqueue(keyChar);
                }
            }
            else
            {
                return;
            }
            await Task.CompletedTask;
        }

        private bool CanMoveNow()
        {
            return _crawlerService.GetState() == ECrawlerStates.ExploreWorld &&
                _party.Combat == null;
        }

        private async Awaitable UpdateMovementInternal(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();

            while (true)
            {
                if (!CanMoveNow() || _movementQueue.Count < 1 || _updatingMovement)
                {
                    await Awaitable.NextFrameAsync(token);
                    continue;
                }

                _updatingMovement = true;
                while (_movementQueue.TryDequeue(out Key currCommand))
                {
                    if (!CanMoveNow())
                    {
                        ClearMovement();
                        break;
                    }

                    MovementKeyCode kc = _movementKeyCodes.FirstOrDefault(x => x.Key == currCommand);
                    if (kc == null)
                    {
                        continue;
                    }

                    CrawlerMoveStatus status = new CrawlerMoveStatus()
                    {
                        KeyCode = kc,
                        World = _world,
                        MapRoot = _mapService.GetMapRoot()
                    };

                    try
                    {
                        List<ICrawlerMoveHelper> helpers = _moveHelpers.OrderedItems().ToList();

                        foreach (ICrawlerMoveHelper helper in helpers)
                        {
                            await helper.Execute(_party, status, token);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Exception(ex, "CrawlerMovement");
                    }

                    if (status.MoveIsComplete)
                    {
                        ClearMovement();
                    }
                }
                _updatingMovement = false;
            }
        }

        public async Awaitable Move(CrawlerMoveStatus status, int forward, int right, CancellationToken token)
        {
            float sin = (float)Math.Round(MathF.Sin(-_party.CurrPos.Rot * Mathf.PI / 180f));
            float cos = (float)Math.Round(Mathf.Cos(-_party.CurrPos.Rot * Mathf.PI / 180f));

            int moveFrames = (int)(_appService.TargetFrameRate / _movesPerSecond);
            float nx = cos * forward + sin * right;
            float nz = sin * forward - cos * right;

            int sx = _party.CurrPos.X;
            int sz = _party.CurrPos.Z;

            int ex = (int)(_party.CurrPos.X + nx);
            int ez = (int)(_party.CurrPos.Z + nz);

            status.SX = sx;
            status.SZ = sz;
            status.EX = ex;
            status.EZ = ez;

            CrawlerMapRoot mapRoot = _mapService.GetMapRoot();

            if (!mapRoot.Map.HasFlag(CrawlerMapFlags.IsLooping))
            {
                if (ex < 0 || ex >= mapRoot.Map.Width ||
                    ez < 0 || ez >= mapRoot.Map.Height)
                {
                    // Bonk
                    await ShowHittingWall(status, token);
                    return;
                }
            }

            status.BlockBits = _mapService.GetBlockingBits(mapRoot.Map, sx, sz, ex, ez, true);

            if (WallTypes.IsBlockingType(status.BlockBits))
            {
                // Bonk
                await ShowHittingWall(status, token);
                return;
            }

            float endDrawX = mapRoot.DrawX + nx * mapRoot.XZBlockSize;
            float endDrawZ = mapRoot.DrawZ + nz * mapRoot.XZBlockSize;

            float startDrawX = mapRoot.DrawX;
            float startDrawZ = mapRoot.DrawZ;

            int frames = moveFrames;

            if (right != 0)
            {
                frames = frames * 1;
            }

            float dz = endDrawZ - startDrawZ;
            float dx = endDrawX - startDrawX;

            int dxgrid = ex - sx;
            int dzgrid = ez - sz;

            int cx = sx;
            int cz = sz;

            bool upperRightOfDoor = false;
            bool openEastDoor = dxgrid != 0;
            if (ex < sx)
            {
                cx = (sx + mapRoot.Map.Width - 1) % mapRoot.Map.Width;
                upperRightOfDoor = true;
            }
            if (ez < sz)
            {
                cz = (sz + mapRoot.Map.Height - 1) % mapRoot.Map.Height;
                upperRightOfDoor = true;
            }

            int assetPosition = (openEastDoor ? DungeonAssetPosition.EastWall : DungeonAssetPosition.NorthWall);


            List<ClientMapCell> allCellsAtMapPos = mapRoot.GetCellsAtMapPos(cx, cz);

            List<DungeonAsset> doorsToOpenClose = new List<DungeonAsset>();

            foreach (ClientMapCell cmc in allCellsAtMapPos)
            {

                DungeonAsset posAsset = cmc.AssetPositions[assetPosition];

                if (posAsset != null)
                {
                    doorsToOpenClose.Add(posAsset);
                }
            }

            List<Awaitable> openList = new List<Awaitable>();

            foreach (DungeonAsset da in doorsToOpenClose)
            {
                openList.Add(da.SetOpened(true, upperRightOfDoor));
            }

            foreach (Awaitable aw in openList)
            {
                await aw;
            }

            _dispatcher.Dispatch(new PlaySound(CrawlerAudio.Footstep));
            for (int frame = 1; frame < frames; frame++)
            {
                mapRoot.DrawX = startDrawX + frame * dx / frames;
                mapRoot.DrawZ = startDrawZ + frame * dz / frames;

                _mapService.UpdateCameraPos(token);

                if (frame == frames * 2 / 3)
                {
                    _dispatcher.Dispatch(new PlaySound(CrawlerAudio.Footstep));
                }

                if (frame < frames - 1)
                {
                    await Task.Delay(1);
                }
            }

            openList.Clear();

            foreach (DungeonAsset da in doorsToOpenClose)
            {
                openList.Add(da.SetOpened(false, upperRightOfDoor));
            }

            foreach (Awaitable aw in openList)
            {
                await aw;
            }


            ex = MathUtil.ModClamp(ex, mapRoot.Map.Width);
            ez = MathUtil.ModClamp(ez, mapRoot.Map.Height);

            _party.CurrPos.X = ex;
            _party.CurrPos.Z = ez;
        }

        private async Awaitable ShowHittingWall(CrawlerMoveStatus status, CancellationToken token)
        {
            status.MoveIsComplete = true;
            status.MovedPosition = false;
            _dispatcher.Dispatch(new ShowFloatingText("Bonk!", EFloatingTextArt.Error));
            ClearMovement();
            await Awaitable.NextFrameAsync(token);
        }

        public async Task Rot(CrawlerMoveStatus status, int delta, bool fastRotate, CancellationToken token)
        {

            float startRot = _party.CurrPos.Rot;
            float endRot = _party.CurrPos.Rot + delta * 90;

            float deltaRot = endRot - startRot;
            int moveFrames = (int)(_appService.TargetFrameRate / _turnsPerSecond);
            int frames = moveFrames * 1;

            if (fastRotate)
            {
                frames = (int)(Math.Max(1, Math.Abs(delta)));
            }

            CrawlerMapRoot mapRoot = _mapService.GetMapRoot();

            for (int frame = 1; frame <= frames; frame++)
            {
                mapRoot.DrawRot = startRot + deltaRot * frame / frames;
                _mapService.UpdateCameraPos(token);
                if (frame < frames)
                {
                    await Task.Delay(1);
                }
            }

            SetFullRot(endRot);

        }

        public void SetFullRot(float endRot)
        {
            _party.CurrPos.Rot = MathUtil.ModClamp((int)endRot, 360);
            _mapService.GetMapRoot().DrawRot = _party.CurrPos.Rot;
        }

        private LastMoveStatus _lastMoveStatus = new LastMoveStatus()
        {
            LastMoveTime = DateTime.UtcNow,
            MovesSinceLastCombat = 0
        };

        public LastMoveStatus GetLastMoveStatus()
        {
            return _lastMoveStatus;
        }
    }
}


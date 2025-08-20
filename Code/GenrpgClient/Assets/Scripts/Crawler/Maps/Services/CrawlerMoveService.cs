using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.MoveHelpers;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Dungeons;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{

    public interface ICrawlerMoveService : IInitializable
    {
        Task AddMovementKeyInput(char keyChar, CancellationToken token);
        void ClearMovement();
        void FinishMove(CrawlerMoveStatus status);
        bool UpdatingMovement();
        Task EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token);
        Awaitable Move(CrawlerMoveStatus status, int forward, int right, CancellationToken token);
        Task Rot(CrawlerMoveStatus status, int delta, bool fastRotate, CancellationToken token);
        LastMoveStatus GetLastMoveStatus();
        void SetFullRot(float endRot);
        IReadOnlyList<MovementKeyCode> GetMovementKeyCodes();

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
        public char Key { get; private set; }
        public int RotationAmount { get; private set; }
        public int ForwardAmount { get; private set; }
        public int RightAmount { get; private set; }
        public string Name { get; private set; }

        public MovementKeyCode(char key, string name, int rotationAmount, int forwardAmount, int rightAmount)
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

        private CancellationToken _token;

        const float _movesPerSecond = 5.0f;

        private PartyData _party = null;
        private CrawlerWorld _world = null;
        private bool _updatingMovement = false;
        const int maxQueuedMoves = 2;
        Queue<char> _movementQueue = new Queue<char>();


        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            SetupMovementKeyCodes();

            _awaitableService.ForgetAwaitable(UpdateMovementInternal(_token));
            await Task.CompletedTask;
        }

        private void SetupMovementKeyCodes()
        {
            _movementKeyCodes = new List<MovementKeyCode>
            {
                new MovementKeyCode('W', MovementKeyNames.Forward, 0, 1, 0),
                new MovementKeyCode((char)273, MovementKeyNames.Forward, 0, 1, 0),

                new MovementKeyCode('S', MovementKeyNames.Backward, 0, -1, 0),
                new MovementKeyCode((char)274, MovementKeyNames.Backward,0, -1, 0),

                new MovementKeyCode('Q', MovementKeyNames.TurnLeft, -1, 0, 0),
                new MovementKeyCode((char)276, MovementKeyNames.TurnLeft, -1, 0, 0),

                new MovementKeyCode('E', MovementKeyNames.TurnRight, 1, 0, 0),
                new MovementKeyCode((char)275, MovementKeyNames.TurnRight, 1, 0, 0),

                new MovementKeyCode('A', MovementKeyNames.StrafeLeft, 0, 0, -1),
                new MovementKeyCode('D', MovementKeyNames.StrafeRight, 0, 0, 1),
            };
        }

        public IReadOnlyList<MovementKeyCode> GetMovementKeyCodes()
        {
            return _movementKeyCodes;
        }

        public async Task EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token)
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

        public async Task AddMovementKeyInput(char keyChar, CancellationToken token)
        {
            if (_movementQueue.Count < maxQueuedMoves)
            {
                if (_movementKeyCodes.Any(x => x.Key == keyChar))
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
                while (_movementQueue.TryDequeue(out char currCommand))
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

            float endDrawX = mapRoot.DrawX + nx * CrawlerMapConstants.XZBlockSize;
            float endDrawZ = mapRoot.DrawZ + nz * CrawlerMapConstants.XZBlockSize;

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

            bool openEastDoor = dxgrid != 0;
            if (ex < sx)
            {
                cx = (sx + mapRoot.Map.Width - 1) % mapRoot.Map.Width;
            }
            if (ez < sz)
            {
                cz = (sz + mapRoot.Map.Height - 1) % mapRoot.Map.Height;
            }

            ClientMapCell mapCell = mapRoot.GetCellAtWorldPos(cx, cz, true);

            int assetPosition = (openEastDoor ? DungeonAssetPosition.EastWall : DungeonAssetPosition.NorthWall);

            DungeonAsset posAsset = mapCell.AssetPositions[assetPosition];

            if (posAsset != null)
            {
                if (posAsset.SetOpened(true))
                {
                    await Task.Delay(100);
                }
            }

            for (int frame = 1; frame < frames; frame++)
            {

                mapRoot.DrawX = startDrawX + frame * dx / frames;
                mapRoot.DrawZ = startDrawZ + frame * dz / frames;

                _mapService.UpdateCameraPos(token);

                if (frame < frames - 1)
                {
                    await Task.Delay(1);
                }
            }

            if (posAsset != null)
            {
                posAsset.SetOpened(false);
            }

            ex = MathUtils.ModClamp(ex, mapRoot.Map.Width);
            ez = MathUtils.ModClamp(ez, mapRoot.Map.Height);

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
            int moveFrames = (int)(_appService.TargetFrameRate / _movesPerSecond);
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
            _party.CurrPos.Rot = MathUtils.ModClamp((int)endRot, 360);
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
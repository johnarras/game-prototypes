using Assets.Scripts.Lockstep.Config.Core;
using Assets.Scripts.Lockstep.Config.Games;
using Assets.Scripts.Lockstep.Factions.Setup;
using Assets.Scripts.Lockstep.Maps.Components;
using Assets.Scripts.Lockstep.Maps.Entities;
using Assets.Scripts.Lockstep.Maps.Setup;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Trader.Biomes.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using static ActorSpawnSystem;
using static UnityEngine.Rendering.STP;

namespace Assets.Scripts.Lockstep.Game.Services
{
    public interface ILockstepGameService : IInitializable
    {
        void SetupGame(BaseLockstepConfig config);
        Task<BaseLockstepConfig> SetupExampleLockstep(long seed);

    }
    public  class LockstepGameService : ILockstepGameService
    {
        private bool _playing = false;

        private IClientUpdateService _updateService = null;
        private ILogService _logService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IClientAppService _appService = null;

        public async Task Initialize(CancellationToken token)
        {

            _updateService.AddUpdate(this, UpdateSimulation, UpdateTypes.Regular, token);

            await Task.CompletedTask;
        }// Internal state


        private List<MapConfig> _maps;

        public BaseLockstepConfig Config { get; private set; }

        private World _world = null;
        private EntityQuery _globalStateQuery;
        private EntityManager _entityManager;

        // Public Getters Only
        public uint CurrentTick { get; private set; }

        // We return a ReadOnlyCollection so the list itself can't be modified from outside
        public ReadOnlyCollection<MapConfig> Maps => _maps.AsReadOnly();

        public void SetupGame(BaseLockstepConfig config)
        {
            try
            {
                CurrentTick = 0;

                _world = new World("LockstepWorld");
                ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(_world);
                    

                Config = config;
                _maps = new List<MapConfig>(config.MapConfigs);

                // 1. Grab the standard Unity Groups
                SimulationSystemGroup simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();
                InitializationSystemGroup initGroup = _world.GetOrCreateSystemManaged<InitializationSystemGroup>();
                PresentationSystemGroup presGroup = _world.GetOrCreateSystemManaged<PresentationSystemGroup>();

                // 2. Create your custom Deterministic Kernel
                LockstepGroup kernelGroup = _world.CreateSystemManaged<LockstepGroup>();

                // 3. Now simGroup is NOT null, so this won't crash
                simGroup.AddSystemToUpdateList(kernelGroup);

                // 3. Register Initialization Systems (Non-deterministic/Input)
                foreach (Type type in config.InitializationPipeline.Systems)
                {
                    AddTypeToGroup(type, initGroup);
                }

                // 4. Register Deterministic Systems into the KERNEL
                // We add the ECB systems inside the kernel so they are part of the strict sequence
                BeginSimulationEntityCommandBufferSystem beginSimECB = _world.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
                EndSimulationEntityCommandBufferSystem endSimECB = _world.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

                kernelGroup.AddSystemToUpdateList(beginSimECB);
                foreach (Type type in config.SimulationPipeline.Systems)
                {
                    AddTypeToGroup(type, kernelGroup);
                }
                kernelGroup.AddSystemToUpdateList(endSimECB);

                // 5. Register Presentation Systems (Visuals)
                foreach (Type type in config.PresentationPipeline.Systems)
                {
                    AddTypeToGroup(type, presGroup);
                }

                // 6. Finalize
                // This sorts the top-level groups, but our kernel preserves internal order
                kernelGroup.SortSystems();
                simGroup.SortSystems();
                initGroup.SortSystems();
                presGroup.SortSystems();

                _entityManager = _world.EntityManager;
                _globalStateQuery = _entityManager.CreateEntityQuery(typeof(LockstepGlobalState));

                Entity globalEntity = _entityManager.CreateEntity
                    (typeof(LockstepGlobalState),
                    typeof(SpawnRequestBuffer));

                _entityManager.SetComponentData(globalEntity, new LockstepGlobalState
                {
                    CurrentTick = 0,
                    DeltaTime = config.FixedDeltaTime,
                    WorldSeed = config.Seed,
                });

                

                config.InjectData(_entityManager);

                MapInjector injector = new MapInjector();   
                injector.InjectData(_entityManager, config.MapConfigs);

                _playing = true;
            }
            catch (Exception e)
            {
                _logService.Exception(e, "LockstepSetup");
            }
        }

        private void AddTypeToGroup(Type type, ComponentSystemGroup group)
        {
            // Check if the type is a Managed System (SystemBase)
            if (typeof(SystemBase).IsAssignableFrom(type))
            {
                // For Managed systems, we use the Managed API
                var managedSystem = _world.GetOrCreateSystemManaged(type);
                group.AddSystemToUpdateList(managedSystem);
            }
            else
            {
                // For Unmanaged systems (ISystem), we use Handles
                SystemHandle handle = _world.CreateSystem(type);
                group.AddSystemToUpdateList(handle);
            }
        }

        public void Update()
        {
            // The simulation heart-beat
            UpdateSimulation();
        }

        public void EndGame()
        {
            if (_world != null && _world.IsCreated)
            {
                _playing = false;
                _world.Dispose();
                _world = null;
                _entityManager = default;
                _globalStateQuery = default;
                Config = null;
                CurrentTick = 0;
                _maps = new List<MapConfig>();
                return;
            }
        }

        private void UpdateSimulation()
        {
            try
            {
                if (!_playing || _world == null || !_world.IsCreated)
                {
                    return;
                }

                CurrentTick++;
                _entityManager.SetComponentData(_globalStateQuery.GetSingletonEntity(), new LockstepGlobalState
                {
                    CurrentTick = this.CurrentTick,
                    DeltaTime = Config.FixedDeltaTime
                });

                _world.Update();

            }
            catch (Exception e)
            {
                _logService.Exception(e, "LockstepService.UpdateSimulation");
            }
        }

        public async Task<BaseLockstepConfig> SetupExampleLockstep(long seed)
        { 

            IRandom rand = new ClientRandom(seed);

            List<FactionConfig> factions = new List<FactionConfig>();
            factions.Add(new FactionConfig()
            {
                FactionId = 1,
                SpawnInterval = 35,
                SpawnPercent = 25,
            });
            factions.Add(new FactionConfig()
            {
                FactionId = 2,
                SpawnInterval = 45,
                SpawnPercent = 30,
            });

            List<MapConfig> maps = new List<MapConfig>();

            int minSize = 15;
            int maxSize = 25;

            int yOffset = maxSize + 10;

            IReadOnlyList<BiomeType> biomes = _gameData.Get<BiomeTypeSettings>(_gs.ch).GetData();

            for (int m = 1; m <= 2; m++)
            {
                int width = MathUtil.IntRange(minSize, maxSize, rand);
                int height = MathUtil.IntRange(minSize, maxSize, rand);

                TileConfig[,] tiles = new TileConfig[width, height];

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        TileConfig tile = new TileConfig()
                        {
                            BiomeTypeId = biomes[rand.Next() % biomes.Count].IdKey,
                        };
                        tiles[x, y] = tile;
                    }
                }

                MapConfig config = new MapConfig()
                {
                    MapId = m,
                    MapName = "Map " + m,
                    Tiles = tiles,
                    Offset = new float2(0, yOffset * (m - 1)),
                    WrapX = rand.Next() % 2 == 0,
                    WrapY = rand.Next() % 2 == 0,
                    CellSize = 1,
                };

                maps.Add(config);
            }

            LockstepConfig lockstepConfig = new LockstepConfig(_appService.TargetFrameRate)
            {
                FactionConfigs = factions,
                MapConfigs = maps,
            };

            await Task.CompletedTask;
            return lockstepConfig;
        }
    }
}

using OxDb.Client.Lockstep.Factions.Setup;
using OxDb.Client.Lockstep.Maps.Entities;
using OxDb.Client.Lockstep.Math;
using OxDb.Client.Lockstep.Spawns;
using OxDb.SharedCore.Core.Constants;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Config.Core
{
    public abstract class BaseLockstepConfig
    {

        public abstract EGameModes GameMode { get; }
        public abstract void SetupSimulationPipeline();

        public uint Seed { get; set; }
        public List<MapConfig> MapConfigs { get; set; } = new List<MapConfig>();

        protected SimulationPipeline _initalizationPipeline = new SimulationPipeline();
        public SimulationPipeline InitializationPipeline => _initalizationPipeline;


        protected SimulationPipeline _simulationPipeline = new SimulationPipeline();
        public SimulationPipeline SimulationPipeline => _simulationPipeline;

        protected SimulationPipeline _presentationPipeline = new SimulationPipeline();
        public SimulationPipeline PresentationPipeline => _presentationPipeline;

        public readonly FixedPoint64 FixedDeltaTime = FixedPoint64.FromInt(1);

        public List<FactionConfig> FactionConfigs { get; set; } = new List<FactionConfig>();

        public NativeList<SpawnRequest> InitialActors { get; set; } = new NativeList<SpawnRequest>();

        public BaseLockstepConfig(int ticksPerSecond)
        {
            FixedDeltaTime = FixedPoint64.FromInt(1) / FixedPoint64.FromInt(ticksPerSecond);
            SetupSimulationPipeline();
        }

        public virtual void InjectData(in EntityManager em)
        {
            FactionInjector fi = new FactionInjector();
            fi.InjectData(em, FactionConfigs);
        }
    }
}
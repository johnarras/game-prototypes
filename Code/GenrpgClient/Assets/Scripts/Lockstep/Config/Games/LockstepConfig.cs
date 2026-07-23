using OxDb.Client.Lockstep.Actors.Systems;
using OxDb.Client.Lockstep.Collisions.Systems;
using OxDb.Client.Lockstep.Config.Core;
using OxDb.Client.Lockstep.Maps.Systems;
using OxDb.Client.Lockstep.Spawns.Systems;
using OxDb.Client.Lockstep.Systems;
using OxDb.SharedCore.Core.Constants;

namespace OxDb.Client.Lockstep.Config.Games
{
    public class LockstepConfig : BaseLockstepConfig
    {
        public override EGameModes GameMode => EGameModes.LockstepTemplate;

        public override void SetupSimulationPipeline()
        {

            _simulationPipeline.AddStep<ActorSpawnSystem>();
            _simulationPipeline.AddStep<FactionSpawnSystem>();
            _simulationPipeline.AddStep<RandomUpdateDirectionSystem>();
            _simulationPipeline.AddStep<BasicMovementSystem>();
            _simulationPipeline.AddStep<BuildSpatialHashSystem>();
            _simulationPipeline.AddStep<CollisionDetectionSystem>();
            _simulationPipeline.AddStep<CollisionResolutionSystem>();


            _simulationPipeline.AddStep<ActorDespawnSystem>();


            _presentationPipeline.AddStep<MapArtSystem>();
            _presentationPipeline.AddStep<ActorVisualSystem>();
        }
        public LockstepConfig(int ticksPerSecond) : base(ticksPerSecond)
        {

        }
    }
}

using Assets.Scripts.Lockstep.Actors.Systems;
using Assets.Scripts.Lockstep.Collisions.Systems;
using Assets.Scripts.Lockstep.Config.Core;
using Assets.Scripts.Lockstep.Maps.Systems;
using Assets.Scripts.Lockstep.Spawns.Systems;
using Assets.Scripts.Lockstep.Systems;
using Genrpg.Shared.Core.Constants;

namespace Assets.Scripts.Lockstep.Config.Games
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

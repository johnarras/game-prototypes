using OxDb.Client.Lockstep.Actors.Components;
using OxDb.Client.Lockstep.Maps.Components;
using OxDb.Client.Lockstep.Systems;
using OxDb.Client.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace OxDb.Client.Lockstep.Actors.Systems
{
    [BurstCompile]
    public partial struct ActorVisualSystem : ISystem, ISeededSystem
    {
        public uint SystemId => SystemSeeds.ActorVisual;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // We need to look up the MapVisualOffset for the map each actor is on
            ComponentLookup<MapVisualOffset> mapOffsets = SystemAPI.GetComponentLookup<MapVisualOffset>(true);

            // Schedule the job to align transforms
            state.Dependency = new ActorVisualJob
            {
                MapOffsets = mapOffsets
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ActorVisualJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<MapVisualOffset> MapOffsets;

            // We update LocalToWorld (provided by Unity.Transforms)
            public void Execute(ref LocalToWorld transform, in ActorPosition simPos, in ActorMap mapRef)
            {
                // 1. Get the map's 3D offset
                float3 worldOffset = float3.zero;
                if (MapOffsets.HasComponent(mapRef.MapEntity))
                {
                    worldOffset = MapOffsets[mapRef.MapEntity].Value;
                }

                // 2. Convert FixedPoint to Float
                // We map Sim-X to World-X and Sim-Z to World-Z (standard top-down)
                float3 visualPos = new float3(
                    (float)simPos.Pos.X,
                    0.0f, // Height is 0 for now
                    (float)simPos.Pos.Z
                );

                // 3. Update the matrix Unity uses for rendering
                // Adding the map offset moves the entire "sim" to its 3D location
                transform.Value = float4x4.Translate(visualPos + worldOffset);
            }
        }
    }
}

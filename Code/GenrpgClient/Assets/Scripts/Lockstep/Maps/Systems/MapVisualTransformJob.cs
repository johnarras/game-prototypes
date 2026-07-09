using Assets.Scripts.Lockstep.Actors.Components;
using Assets.Scripts.Lockstep.Maps.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct MapVisualTransformJob : IJobEntity
{
    // We look up the offset based on the MapId the actor belongs to
    [ReadOnly] public ComponentLookup<MapVisualOffset> MapOffsets;

    public void Execute(ref LocalToWorld transform, in ActorPosition simPos, in ActorMap actorMap)
    {
        // 1. Get the 3D offset for the map this actor is on
        float3 offset = MapOffsets[actorMap.MapEntity].Value;

        // 2. Convert Sim Position to Visual Position
        float3 visualPos = new float3(
            (float)simPos.Pos.X,
            0,
            (float)simPos.Pos.Z
        );

        // 3. Final World Position = Sim + Offset
        transform.Value = float4x4.TRS(
            visualPos + offset,
            quaternion.identity,
            new float3(1, 1, 1)
        );
    }
}
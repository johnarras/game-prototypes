using Assets.Scripts.Lockstep.Actors.Components;
using Assets.Scripts.Lockstep.Collisions.Components;
using Assets.Scripts.Lockstep.Collisions.Constants;
using Assets.Scripts.Lockstep.Factions.Components;
using Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Systems;
using Assets.Scripts.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Collisions.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct CollisionDetectionSystem : ISystem, ISeededSystem
    {
        public uint SystemId => SystemSeeds.CollisionDetection;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatialMapSingleton = SystemAPI.GetSingleton<SpatialHashSingleton>();

            // Clear buffers before detection
            state.Dependency = new ClearCollisionBuffersJob().ScheduleParallel(state.Dependency);

            // Run Narrow Phase
            state.Dependency = new NarrowPhaseJob
            {
                SpatialMap = spatialMapSingleton.SpatialMap,
                CellSize = spatialMapSingleton.CellSize,
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct ClearCollisionBuffersJob : IJobEntity
    {
        public void Execute(ref DynamicBuffer<CollisionBuffer> buffer) => buffer.Clear();
    }

    [BurstCompile]
    public partial struct NarrowPhaseJob : IJobEntity
    {
        [ReadOnly] public NativeParallelMultiHashMap<int, SpatialEntry> SpatialMap; //
        [ReadOnly] public int CellSize;

        public void Execute(Entity entity, [ReadOnly] DynamicBuffer<CollisionBuffer> buffer,
            in ActorPosition pos, in ActorRotation rot, in CollisionShape shape, in ActorFaction faction)
        {
            int hash = SpatialMath.GetCellHash(pos.Pos, SpatialMath.DefaultCellSize); //

            if (SpatialMap.TryGetFirstValue(hash, out SpatialEntry neighbor, out var it))
            {
                do
                {
                    // 1. Skip self
                    if (neighbor.Actor == entity) continue;

                    // 2. Faction & Category Filtering
                    if (neighbor.FactionId == faction.FactionId) continue;

                    // 3. Precise Math Branching
                    if (CheckCollision(pos.Pos, shape, rot.Angle, neighbor, out CollisionBuffer newBuffer))
                    {
                        buffer.Add(newBuffer);
                    }
                } while (SpatialMap.TryGetNextValue(out neighbor, ref it));
            }
        }

        private bool CheckCollision(Vector2Fixed myPos, CollisionShape myShape, FixedPoint64 myAngle,
                             SpatialEntry other, out CollisionBuffer result)
        {
            result = default; // Initialize for out parameter

            // Case A: Circle vs Circle (Most common)
            if (myShape.ShapeType == ECollisionShapeType.Circle && other.Shape.ShapeType == ECollisionShapeType.Circle)
            {
                Vector2Fixed diff = myPos - other.Pos;
                FixedPoint64 distSq = (diff.X * diff.X) + (diff.Z * diff.Z);
                FixedPoint64 radiusSum = myShape.Extents.X + other.Shape.Extents.X;

                if (distSq < (radiusSum * radiusSum))
                {
                    // Deterministic jitter for perfect overlap
                    if (distSq.RawValue == 0)
                    {
                        result.Normal = new Vector2Fixed(FixedPoint64.FromRaw(1), FixedPoint64.FromInt(0));
                        result.Penetration = radiusSum;
                    }
                    else
                    {
                        FixedPoint64 dist = FixedPointMath.Sqrt(distSq);
                        result.Normal = diff / dist;
                        result.Penetration = radiusSum - dist;
                    }
                    result.CollidedWith = other.Actor;
                    return true;
                }
            }

            // Case B: Circle vs OBB (Rotating Laser)
            if (myShape.ShapeType == ECollisionShapeType.Circle && other.Shape.ShapeType == ECollisionShapeType.Rectangle)
            {
                // IntersectCircleOBB would now return the Normal/Penetration inside the 'result' struct
                return IntersectCircleOBB(myPos, myShape.Extents.X, other, out result);
            }

            return false;
        }

        private FixedPoint64 GetDistSq(Vector2Fixed a, Vector2Fixed b)
        {
            FixedPoint64 dx = a.X - b.X;
            FixedPoint64 dz = a.Z - b.Z;
            return (dx * dx) + (dz * dz);
        }

        private bool IntersectCircleOBB(Vector2Fixed circlePos, FixedPoint64 radius, SpatialEntry rectEntry, out CollisionBuffer result)
        {
            result = default;

            // 1. Calculate relative position
            Vector2Fixed relative = circlePos - rectEntry.Pos; //

            // 2. Rotate into Rectangle's Local Space
            // We use the rectangle's Angle from its ActorRotation/SpatialEntry
            FixedPoint64 cos = FixedPointMath.Cos(rectEntry.Shape.Extents.X); // Assuming angle is stored here or passed in
            FixedPoint64 sin = FixedPointMath.Sin(rectEntry.Shape.Extents.X);

            // Standard 2D rotation matrix for local space transformation
            FixedPoint64 localX = (relative.X * cos) + (relative.Z * sin); //
            FixedPoint64 localZ = (-relative.X * sin) + (relative.Z * cos);

            // 3. Find Closest Point on AABB in local space
            // Rectangle extents are stored in Extents (Half-Width, Half-Height)
            FixedPoint64 closestX = FixedPoint64.Max(-rectEntry.Shape.Extents.X, FixedPoint64.Min(localX, rectEntry.Shape.Extents.X));
            FixedPoint64 closestZ = FixedPoint64.Max(-rectEntry.Shape.Extents.Z, FixedPoint64.Min(localZ, rectEntry.Shape.Extents.Z));

            // 4. Calculate Distance and Normal in local space
            FixedPoint64 localDiffX = localX - closestX;
            FixedPoint64 localDiffZ = localZ - closestZ;
            FixedPoint64 distSq = (localDiffX * localDiffX) + (localDiffZ * localDiffZ);

            if (distSq < (radius * radius))
            {
                FixedPoint64 dist = FixedPointMath.Sqrt(distSq); //

                // Handle perfect center overlap
                if (dist.RawValue == 0)
                {
                    result.Normal = new Vector2Fixed(FixedPoint64.FromInt(0), FixedPoint64.FromRaw(1));
                    result.Penetration = radius;
                }
                else
                {
                    // Local normal
                    Vector2Fixed localNormal = new Vector2Fixed(localDiffX / dist, localDiffZ / dist);

                    // 5. Transform Normal back to World Space
                    // WorldNormal.X = localX * cos - localZ * sin
                    // WorldNormal.Z = localX * sin + localZ * cos
                    result.Normal = new Vector2Fixed(
                        (localNormal.X * cos) - (localNormal.Z * sin),
                        (localNormal.X * sin) + (localNormal.Z * cos)
                    );

                    result.Penetration = radius - dist;
                }

                result.CollidedWith = rectEntry.Actor;
                return true;
            }

            return false;
        }
    }
}
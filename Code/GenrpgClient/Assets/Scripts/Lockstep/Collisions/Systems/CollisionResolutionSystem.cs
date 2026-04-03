using Assets.Scripts.Lockstep.Actors.Components;
using Assets.Scripts.Lockstep.Buildings.Components;
using Assets.Scripts.Lockstep.Collisions.Components;
using Assets.Scripts.Lockstep.Math;
using Assets.Scripts.Lockstep.Projectiles.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Collisions.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct CollisionResolutionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // We need RO lookups to check neighbor data during resolution
            var positions = SystemAPI.GetComponentLookup<ActorPosition>(true);
            var shapes = SystemAPI.GetComponentLookup<CollisionShape>(true);
            var buildings = SystemAPI.GetComponentLookup<BuildingTag>(true);
            var projectiles = SystemAPI.GetComponentLookup<ProjectileTag>(true);

            var resolveJob = new CollisionResolutionJob
            {
                Positions = positions,
                Shapes = shapes,
                Buildings = buildings,
                Projectiles = projectiles,
                BuildingLookup = state.GetComponentLookup<BuildingTag>(true)
            };

            state.Dependency = resolveJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithNone(typeof(ProjectileTag))] // Optimization: Resolution ignores projectiles as the primary subject
    public partial struct CollisionResolutionJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<ActorPosition> Positions;
        [ReadOnly] public ComponentLookup<CollisionShape> Shapes;
        [ReadOnly] public ComponentLookup<BuildingTag> Buildings;
        [ReadOnly] public ComponentLookup<ProjectileTag> Projectiles;

        [ReadOnly] public ComponentLookup<BuildingTag> BuildingLookup;

        public void Execute(Entity entity, ref ActorPosition myPos, in DynamicBuffer<CollisionBuffer> collisions)
        {
            if (BuildingLookup.HasComponent(entity))
            {
                return;
            }

            Vector2Fixed totalPush = Vector2Fixed.Zero;

            for (int i = 0; i < collisions.Length; i++)
            {
                var collision = collisions[i];

                // Commutative addition for determinism
                // If 'other' is a Building, use 100% penetration, otherwise 50%
                FixedPoint64 weight = GetWeight(entity, collision.CollidedWith);
                totalPush += collision.Normal * (collision.Penetration * weight);
            }

            myPos.Pos += totalPush;
        }
        private FixedPoint64 GetWeight(Entity me, Entity other)
        {

            if (BuildingLookup.HasComponent(other))
            {
                return FixedPoint64.FromInt(1);
            }

            // If we are both units, we each take 50%
            return FixedPoint64.FromRaw(FixedPoint64.Half); // 0.5 in fixed-point
        }
    }
}
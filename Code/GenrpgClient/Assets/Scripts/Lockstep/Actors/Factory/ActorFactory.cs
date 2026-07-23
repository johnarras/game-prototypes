using OxDb.Client.Lockstep.Actors.Components;
using OxDb.Client.Lockstep.Actors.Constants;
using OxDb.Client.Lockstep.Buildings.Components;
using OxDb.Client.Lockstep.Factions.Components;
using OxDb.Client.Lockstep.Maps.Components;
using OxDb.Client.Lockstep.Projectiles.Components;
using OxDb.Client.Lockstep.Spawns;
using OxDb.Client.Lockstep.Units.Components;
using Unity.Burst;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Actors.Factory
{
    [BurstCompile]
    public static class ActorFactory
    {
        /// <summary>
        ///  THIS MUST NOT USE ANYTHING THAT WAS NOT PASSED INTO IT TO ALLOW IT TO BE DONE IN PARALLEL!
        /// </summary>
        /// <param name="sortKey"></param>
        /// <param name="entity"></param>
        /// <param name="ecb"></param>
        /// <param name="req"></param>
        /// <param name="nextActorId"></param>
        /// <param name="currentTick"></param>
        [BurstCompile]
        public static void ExecuteSpawn(int sortKey, in Entity entity, in EntityCommandBuffer.ParallelWriter ecb, in SpawnRequest req, uint nextActorId, uint currentTick)
        {

            // --- STAGE 1: THE BASE PACKAGE ---
            // Every actor in the sim gets these 4 components
            ecb.AddComponent(sortKey, entity, new ActorId { Value = nextActorId });
            ecb.AddComponent(sortKey, entity, new ActorPosition { Pos = req.Position });
            ecb.AddComponent(sortKey, entity, new ActorRotation { Angle = req.Angle });
            ecb.AddComponent(sortKey, entity, new ActorFaction
            {
                FactionId = req.FactionId,
                // You might look up the Entity based on FactionId here
            });


            ecb.AddComponent(sortKey, entity, req.Shape);

            if (req.TTLTicks > 0)
            {
                ecb.AddComponent(sortKey, entity, new Lifetime() { ExpiryTick = currentTick + req.TTLTicks });
            }

            // Add MapAssignment so the Spatial Hash knows where it lives
            ecb.AddComponent(sortKey, entity, new ActorMap { MapEntity = req.MapEntity });

            // --- STAGE 2: SPECIALIZATION ---
            // Now add the Tags and extra data based on Category
            switch (req.Category)
            {
                case EActorCategories.Unit:
                    ecb.AddComponent<UnitTag>(sortKey, entity);
                    ecb.AddComponent(sortKey, entity, new ActorSpeed { Speed = req.UnitData.Speed });
                    break;
                case EActorCategories.Building:
                    ecb.AddComponent<BuildingTag>(sortKey, entity);
                    break;
                case EActorCategories.Projectile:
                    ecb.AddComponent<ProjectileTag>(sortKey, entity);
                    ecb.AddComponent(sortKey, entity, new ActorSpeed { Speed = req.ProjectileData.Speed });
                    break;
            }

        }
    }
}

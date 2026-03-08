using Assets.Scripts.Lockstep.Maps.Components;
using Assets.Scripts.Lockstep.Presentation.Services;
using Assets.Scripts.Lockstep.Systems;
using Assets.Scripts.Lockstep.Systems.Constants;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Lockstep.Maps.Systems
{
    [DisableAutoCreation]
    public partial struct MapArtSystem : ISeededSystem, ISystem
    {
        public uint SystemId => SystemSeeds.MapArt;

        public void OnUpdate(ref SystemState state)
        {
            // Use an ECB to record the 'MapArtCreated' tag assignment
            // to avoid structural change sync points during the loop.
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            // Explicitly typed query for entities missing the art tag
            // RefRO<T> = Read-Only reference, RefRW<T> = Read-Write reference
            foreach (var (activeMap, entity) in
                     SystemAPI.Query<RefRO<ActiveMap>>()
                     .WithNone<MapArtCreated>()
                     .WithEntityAccess())
            {
                // Accessing the BlobAssetReference through ValueRO
                ref MapBlob map = ref activeMap.ValueRO.MapRef.Value;

                float3 worldOffset = float3.zero;
                if (SystemAPI.HasComponent<MapVisualOffset>(entity))
                {
                    worldOffset = SystemAPI.GetComponent<MapVisualOffset>(entity).Value;
                }

                // Call internal helper to spawn GameObjects or Meshes
                SpawnMapVisuals(ref map, worldOffset);

                // Add the tag to ensure we don't process this map again
                ecb.AddComponent<MapArtCreated>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void SpawnMapVisuals(ref MapBlob map, float3 worldOffset)
        {
            int width = map.Size.x;
            int height = map.Size.y;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileData tile = map.Tiles[y * width + x];

                    // Convert sim position to visual position including the map's offset
                    float3 visualPos = new float3(
                        (float)(x * (int)map.CellSize) + worldOffset.x,
                        worldOffset.y,
                        (float)(y * (int)map.CellSize) + worldOffset.z
                    );

                    LockstepVisualFactory.Instance.SpawnMapTile(tile.BiomeTypeId, visualPos, map.CellSize);

                    TileVisualFactory.Spawn(tile.BiomeTypeId, visualPos);
                }
            }
        }
    }
}
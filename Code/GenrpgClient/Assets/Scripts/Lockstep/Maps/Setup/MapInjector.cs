using Assets.Scripts.Lockstep.Maps.Components;
using Assets.Scripts.Lockstep.Maps.Entities;
using Assets.Scripts.Lockstep.Math;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Lockstep.Maps.Setup
{
    public class MapInjector
    {
        public void InjectData(in EntityManager em, List<MapConfig> maps)
        {

            NativeArray<BlobAssetReference<MapBlob>> tempBlobs = new NativeArray<BlobAssetReference<MapBlob>>(maps.Count, Allocator.Temp);
            for (int i = 0; i < maps.Count; i++)
            {

                tempBlobs[i] = CreateBlob(maps[i]);
            }

            // 2. Use a Builder to create the "Library" Blob
            using var builder = new BlobBuilder(Allocator.Temp);

            // We are building a BlobArray<BlobAssetReference<MapBlob>>
            ref BlobArray<BlobAssetReference<MapBlob>> root = ref builder.ConstructRoot<BlobArray<BlobAssetReference<MapBlob>>>();

            // Allocate the space in the blob
            var arrayBuilder = builder.Allocate(ref root, tempBlobs.Length);
            for (int i = 0; i < tempBlobs.Length; i++)
            {
                arrayBuilder[i] = tempBlobs[i];
            }

            // Create the final reference
            var libraryRef = builder.CreateBlobAssetReference<BlobArray<BlobAssetReference<MapBlob>>>(Allocator.Persistent);

            // 3. Create the entity and assign
            Entity entity = em.CreateEntity();
            em.AddComponentData(entity, new MapLibrary
            {
                Maps = libraryRef
            });

            for (int i = 0; i < libraryRef.Value.Length; i++)
            {
                Entity mapEntity = em.CreateEntity();

                // The "Key" for logic systems to find map data
                em.AddComponentData(mapEntity, new ActiveMap
                {
                    MapRef = libraryRef.Value[i]
                });

                float2 offset = libraryRef.Value[i].Value.Offset;
                // The "Key" for the presentation system to place the art
                // Overworld at (0,0,0), Underworld at (0, -1000, 0), etc.
                em.AddComponentData(mapEntity, new MapVisualOffset
                {
                    Value = new float3(offset.x, offset.y, 0),
                });

                em.SetName(mapEntity, "Map " + libraryRef.Value[i].Value.MapName);
            }


            tempBlobs.Dispose();
        }

        private BlobAssetReference<MapBlob> CreateBlob(MapConfig config)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref MapBlob root = ref builder.ConstructRoot<MapBlob>();

            int width = config.Tiles.GetLength(0);
            int height = config.Tiles.GetLength(1);

            // 1. Set basic properties
            root.Size = new int2(width, height);
            root.WrapX = (byte)(config.WrapX ? 1 : 0);
            root.WrapZ = (byte)(config.WrapZ ? 1 : 0);
            root.CellSize = config.CellSize;

            root.MapId = config.MapId;
            root.MapName = config.MapName;
            root.Offset = config.Offset;

            // Assuming FixedPoint64 conversion exists in your Math lib
            root.WorldWidth = FixedPoint64.FromInt(width * config.CellSize);
            root.WorldHeight = FixedPoint64.FromInt(height * config.CellSize);

            // 2. Flatten 2D array into 1D BlobArray
            BlobBuilderArray<TileData> tileArray = builder.Allocate(ref root.Tiles, width * height);

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (z * width) + x;
                    var tileConfig = config.Tiles[x, z];

                    tileArray[index] = new TileData
                    {
                        BiomeTypeId = (ushort)tileConfig.BiomeTypeId,
                        MoveCost = (byte)tileConfig.MoveCost,
                        Flags = tileConfig.Flags,
                    };
                }
            }

            return builder.CreateBlobAssetReference<MapBlob>(Allocator.Persistent);
        }
    }
}

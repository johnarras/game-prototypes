
using Assets.Scripts.Lockstep.Math;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.Lockstep.Maps.Components
{
    [GenerateTestsForBurstCompatibility]
    public struct TileData
    {
        public ushort BiomeTypeId;   // 2 bytes: The visual/type identity
        public byte MoveCost;        // 1 byte: 0 = Blocked, 1 = Fastest, 255 = Heavily Slowed
        public TileFlags Flags;      // 1 byte: Bitmask for vision, liquid, etc.
    }

    [System.Flags]
    public enum TileFlags : byte
    {
        None = 0,
        BlocksVision = 1 << 0,
        IsLiquid = 1 << 1,
        IsBurnable = 1 << 2
    }

    [GenerateTestsForBurstCompatibility]
    public struct MapBlob
    {
        public int MapId; // Explicit ID
        public FixedString32Bytes MapName; // For UI/Debug
        public int2 Size;
        public byte WrapX;
        public byte WrapZ;
        public float2 Offset;

        // Use FixedPoint64 for map boundaries/scale to avoid floats

        public int CellSize;
        public FixedPoint64 WorldWidth;
        public FixedPoint64 WorldHeight;

        public BlobArray<TileData> Tiles;
    }

    public static class MapBlobUtils
    {
        public static TileData GetTile(this ref MapBlob map, int x, int z)
        {
            // Optional: Handle Wrapping
            if (map.WrapX == 1) x = (x % map.Size.x + map.Size.x) % map.Size.x;
            if (map.WrapZ == 1) z = (z % map.Size.y + map.Size.y) % map.Size.y;

            int index = (z * map.Size.x) + x;
            return map.Tiles[index];
        }
    }

    public struct ActiveMap : IComponentData
    {
        public BlobAssetReference<MapBlob> MapRef;
    }
}

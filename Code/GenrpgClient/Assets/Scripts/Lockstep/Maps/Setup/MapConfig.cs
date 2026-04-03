namespace Assets.Scripts.Lockstep.Maps.Entities
{
    using global::Assets.Scripts.Lockstep.Maps.Components;
    using Unity.Mathematics;

    public class MapConfig
    {
        public int MapId;
        public string MapName;

        public bool WrapX;
        public bool WrapY;

        public float2 Offset;

        public int CellSize = 1;

        public TileConfig[,] Tiles { get; set; }

        public MapConfig()
        {
        }

    }

    public class TileConfigConstants
    {
        public const byte Unwalkable = 0;
        public const byte DefaultWalkable = 1;
    }

    // Would like to be able to show an image 
    public class TileConfig
    {
        public long BiomeTypeId { get; set; }

        public byte MoveCost { get; set; } = TileConfigConstants.DefaultWalkable;

        public TileFlags Flags { get; set; }
    }
}
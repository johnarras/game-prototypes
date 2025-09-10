using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Zones.Settings;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class CellIndex
    {
        public const int Dir = 0;
        public const int Walls = 1;
        public const int Terrain = 2;
        public const int Region = 3;
        public const int EntityType = 4;
        public const int EntityId = 5;
        public const int Max = 6;
    }

    [MessagePackObject]
    public class CrawlerMap : IStringId
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public List<ZoneRegion> Regions { get; set; } = null;
        [Key(4)] public long CrawlerMapTypeId { get; set; } = CrawlerMapTypes.Dungeon;
        [Key(5)] public int Width { get; set; }
        [Key(6)] public int Height { get; set; }
        [Key(7)] public int Level { get; set; }
        [Key(8)] public int LevelDelta { get; set; }
        [Key(9)] public long MapFloor { get; set; }
        [Key(10)] public string FromPlaceName { get; set; }
        [Key(11)] public long MapQuestItemId { get; set; }
        [Key(12)] public MapEntranceRiddle EntranceRiddle { get; set; }
        [Key(13)] public MapRiddleHints RiddleHints { get; set; }
        [Key(14)] public byte[] Data { get; set; }
        [Key(15)] public long ArtSeed { get; set; }
        [Key(16)] public long WeatherTypeId { get; set; }
        [Key(17)] public long ZoneTypeId { get; set; }
        [Key(18)] public long BuildingTypeId { get; set; }
        [Key(19)] public long BuildingArtId { get; set; }
        [Key(20)] public long BaseCrawlerMapId { get; set; }
        [Key(21)] public List<MapCellDetail> Details { get; set; } = new List<MapCellDetail>();
        [Key(22)] public List<ZoneUnitSpawn> ZoneUnits { get; set; } = new List<ZoneUnitSpawn>();
        [Key(23)] public List<CurrentUnitKeyword> UnitKeywords { get; set; } = new List<CurrentUnitKeyword>();

        [Key(24)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public void SetupDataBlocks()
        {
            Data = new byte[Width * Height * CellIndex.Max];
        }

        public byte Get(int x, int z, int offset)
        {
            return Data[GetDataIndex(x, z, offset)];
        }

        public byte GetEntityId(int x, int z, long entityTypeId)
        {
            if (entityTypeId < 1 || Get(x, z, CellIndex.EntityType) != entityTypeId)
            {
                return 0;
            }
            return Get(x, z, CellIndex.EntityId);
        }

        public void SetEntity(int x, int z, long entityTypeId, long entityId)
        {
            if (entityTypeId == 0 || entityId == 0)
            {
                Set(x, z, CellIndex.EntityType, 0);
                Set(x, z, CellIndex.EntityId, 0);
            }
            else
            {
                Set(x, z, CellIndex.EntityType, entityTypeId);
                Set(x, z, CellIndex.EntityId, entityId);
            }
        }

        public void Set(int x, int z, int offset, long value)
        {
            Data[GetDataIndex(x, z, offset)] = (byte)value;
        }

        public void AddBits(int x, int z, int offset, long value)
        {
            Data[GetDataIndex(x, z, offset)] |= (byte)value;
        }

        protected int GetDataIndex(int x, int z, int offset)
        {
            return offset * Width * Height + z * Width + x;
        }

        public int GetIndex(int x, int z)
        {
            return z * Width + x;
        }


        private string _floorName = null;
        public string GetName(int x, int z)
        {
            if (MapFloor > 1)
            {

                if (string.IsNullOrEmpty(_floorName))
                {
                    _floorName = Name + " (Level " + MapFloor + ")";
                }
                return _floorName;
            }

            if (Regions == null || Regions.Count < 1)
            {
                return Name;
            }

            byte ztype = Get(x, z, CellIndex.Region);

            ZoneRegion region = Regions.FirstOrDefault(x => x.ZoneTypeId == ztype);
            if (region != null)
            {
                return region.Name;
            }
            return Name;
        }

        public byte EastWall(int x, int y)
        {
            return (byte)((Get(x, y, CellIndex.Walls) >> MapWallBits.EWallStart) % (1 << MapWallBits.WallBitSize));
        }

        public byte NorthWall(int x, int y)
        {
            return (byte)((Get(x, y, CellIndex.Walls) >> MapWallBits.NWallStart) % (1 << MapWallBits.WallBitSize));
        }

        public bool EntranceRiddleRequired()
        {
            return EntranceRiddle != null &&
                !string.IsNullOrEmpty(EntranceRiddle.Text) &&
                !string.IsNullOrEmpty(EntranceRiddle.Answer);
        }

        public bool IsValidEmptyCell(int x, int z)
        {
            return Get(x, z, CellIndex.Terrain) > 0 &&
                Get(x, z, CellIndex.EntityType) == 0 &&
                !Details.Any(d => d.X == x && d.Z == z);
        }

        public List<MapEntity> GetMapEntities(long entityTypeId, long entityId = 0)
        {
            List<MapEntity> retval = new List<MapEntity>();

            if (entityTypeId < 1)
            {
                return retval;
            }

            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Height; z++)
                {
                    if (Get(x, z, CellIndex.EntityType) == entityTypeId)
                    {
                        long currEntityId = Get(x, z, CellIndex.EntityId);
                        if ((entityId == 0 && currEntityId != 0) || (entityId > 0 && (currEntityId == entityId)))
                        {
                            retval.Add(new MapEntity()
                            {
                                EntityTypeId = entityTypeId,
                                EntityId = currEntityId,
                                X = x,
                                Z = z
                            });
                        }
                    }
                }
            }

            return retval;
        }

        public int GetMapLevelAtPoint(int x, int z)
        {
            if (CrawlerMapTypeId != CrawlerMapTypes.Outdoors ||
                Regions == null || Regions.Count < 1 || LevelDelta < 1)
            {
                return Level;
            }

            ZoneRegion region = Regions.OrderBy(x => x.Level).First();

            bool smallXStart = region.CenterX < Width / 2;
            bool smallZStart = region.CenterY < Height / 2;

            float xPercent = (smallXStart ? x : Width - 1 - x) * 1.0f / Width;
            float zPercent = (smallZStart ? z : Height - 1 - z) * 1.0f / Height;

            float totalPercent = (xPercent + zPercent) / 2;

            totalPercent *= totalPercent;

            return (int)(Level + totalPercent * LevelDelta);
        }
    }
}

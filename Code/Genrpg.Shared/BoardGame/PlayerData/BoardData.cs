using MessagePack;
using Genrpg.Shared.BoardGame.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Pvp.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
namespace Genrpg.Shared.BoardGame.PlayerData
{
    public class DataArray
    {
        public short[] Data = new short[0];

        public long this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = (short)value; }
        }

        public int Length
        {
            get { return Data.Length; }
        }

        public void Create(int length)
        {
            Data = new short[length];
        }
    }


    [MessagePackObject]
    public class BoardData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public long BoardModeId { get; set; }
        [Key(3)] public long ZoneTypeId { get; set; }
        [Key(4)] public long Seed { get; set; }
        [Key(5)] public int TileIndex { get; set; }

        [Key(6)] public int ModeStartIndex { get; set; }
        [Key(7)] public int ModeRollsLeft { get; set; }
        [Key(8)] public int ModeLapsLeft { get; set; }

        [Key(9)] public int Width { get; set; }
        [Key(10)] public int Height { get; set; }
        [Key(11)] public int Length { get; set; }

        [Key(12)] public int StartUpgradeTier { get; set; }
        [Key(13)] public int UpgradeTierCount { get; set; }

        [Key(14)] public SmallIndexBitList PathPositions { get; set; } = new SmallIndexBitList();

        [Key(15)] public SmallIdShortCollection Tiles { get; set; } = new SmallIdShortCollection();

        [Key(16)] public SmallIdShortCollection PassPrizes { get; set; } = new SmallIdShortCollection();
        [Key(17)] public SmallIdShortCollection LandPrizes { get; set; } = new SmallIdShortCollection();

        [Key(18)] public SmallIdShortCollection TileTypeUpgradeTiers { get; set; } = new SmallIdShortCollection();

        // Store this on board for use with rolling.
        [Key(19)] public PvpSharedData PVPData { get; set; }

        [Key(20)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        private long[] _allPathIndexes = null;
        public long[] GetAllPathIndexes()
        {
            if (_allPathIndexes != null)
            {
                return _allPathIndexes;
            }
            long[] retval = new long[Length];
            long pathIndex = BoardGameConstants.StartPathIndex;
            retval[0] = pathIndex;
            for (int t = 1; t < Length; t++)
            {
                retval[t] = pathIndex;
            }
            _allPathIndexes = retval;
            return retval;
        }

        public long GetPathIndex(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= Length)
            {
                return BoardGameConstants.StartPathIndex;
            }
            long pathIndex = BoardGameConstants.StartPathIndex;
            for (int t = 1; t < Length && t < tileIndex; t++)
            {
            }
            return pathIndex;
        }

        public void Clear()
        {
            Tiles = new SmallIdShortCollection();
            PassPrizes = new SmallIdShortCollection();
            LandPrizes = new SmallIdShortCollection();
            PathPositions = new SmallIndexBitList();
            TileTypeUpgradeTiers = new SmallIdShortCollection();
            Flags = 0;
            TileIndex = BoardGameConstants.FirstTileIndex;
            
        }

        public int GetTotalUpgradeTier(long tileTypeId)
        {
            if (tileTypeId < 0 || tileTypeId >= TileTypeUpgradeTiers.GetLength())
            {
                return 0;
            }

            return GetCurrentUpgradeTier(tileTypeId) + StartUpgradeTier;
        }

        public bool IsOwnBoard()
        {
            return BoardModeId == BoardModes.Primary;
        }

        public int GetCurrentUpgradeTier(long tileTypeId)
        {
            return TileTypeUpgradeTiers.Get(tileTypeId);
        }

        private bool OkCoordinate(int x, int z)
        {
            return x >= 0 && z >= 0 && x < Width && z < Height;
        }

        public bool IsOnPath(int x, int z)
        {
            if (!OkCoordinate(x, z))
            {
                return false;
            }
            return PathPositions.HasBit(GetIndexFromGrid(x, z));
        }

        public void SetIsOnPath(int x, int z, bool onPath)
        {
            if (!OkCoordinate(x,z))
            {
                return;
            }
            if (onPath)
            {
                PathPositions.SetBit(GetIndexFromGrid(x, z));
            }
            else
            {
                PathPositions.RemoveBit(GetIndexFromGrid(x, z));
            }
        }

        public bool IsValid()
        {
            int pathTilesFound = 0;
            for (int x = 0;  x < Width; x++)
            {
                for (int z = 0; z < Height; z++)
                {
                    if (!IsOnPath(x,z))
                    {
                        continue;
                    }

                    int adjacentPathCount = 0;

                    if (IsOnPath(x-1,z))
                    {
                        adjacentPathCount++;
                    }
                    if (IsOnPath(x+1,z))
                    {
                        adjacentPathCount++;
                    }
                    if (IsOnPath(x,z-1))
                    {
                        adjacentPathCount++;
                    }
                    if (IsOnPath(x,z+1))
                    {
                        adjacentPathCount++;
                    }
                    if (adjacentPathCount != 2)
                    {
                        return false;
                    }
                    pathTilesFound++;
                }
            }

            return pathTilesFound % 7 != 0 &&
                pathTilesFound == Length;
        }

        private int GetIndexFromGrid(int x, int z)
        {
            return x + z * Width;
        }


        public bool IsClockwise()
        {
            return Seed % 2 == 0;
        }

        public PointXZ GetStartPos()
        {
            for (int z = 0; z < Height; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (IsOnPath(x,z))
                    {
                        int startX = x;
                        int startZ = z;
                        for (int x1 = x + 1; x1 < Width; x1++)
                        {
                            if (IsOnPath(x1,z))
                            {
                                startX = x1;
                            }
                        }
                        return new PointXZ(startX, startZ);
                    }
                }
            }
            return null;
        }
    }
    public class BoardDataLoader : UnitDataLoader<BoardData> { }

    public class BoardDto : NoChildPlayerDataDto<BoardData> { }


    public class BoardDataMapper : NoChildUnitDataMapper<BoardData, BoardDto> { }
}

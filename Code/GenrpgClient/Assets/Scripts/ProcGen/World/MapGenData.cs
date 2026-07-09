using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.MapServer.Services;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;

public class ExtendedWorldObjectData
{
    public long X { get; set; }
    public long Z { get; set; }

    public int XSize { get; set; }

    public int ZSize { get; set; }

    public long EntityTypeId { get; set; }

    public long EntityId { get; set; }
    public int Height { get; set; }

    public ushort Angle { get; set; }

    public ushort HAngle { get; set; }
}


public interface IMapGenData : IInjectable
{

    // Alphamap width and height
    int Awid { get; set; }
    int Ahgt { get; set; }

    // Detail map width and height
    int DWid { get; set; }
    int DHgt { get; set; }

    byte[,,] GrassAmounts { get; set; }

    // heightmap
    float[,] Heights { get; set; }
    float[,] SubZonePercents { get; set; }
    int[,] SubZoneIds { get; set; }
    float[,] OverrideZoneScales { get; set; }
    // splatmaps
    float[,,] Alphas { get; set; }

    float[,] RoadDistances { get; set; }


    // List of roads created
    List<List<LineCell>> Roads { get; set; }

    List<Point2I> CreviceBridges { get; set; }
    // Bridges that have been made
    List<Point2I> CurrBridges { get; set; }

    public ExtendedWorldObjectData[,] ExtendedObjects { get; set; }

    ushort[,] BridgeDistances { get; set; }


    float[,] MountainNoise { get; set; }
    float[,] MountainDecayPower { get; set; }

    List<Location>[,] LocationGrid { get; set; }

    // Ends of ramps where special monsters or quests can be placed.
    List<Point2F> RampTops { get; set; }

    float[,] CreviceDepths { get; set; }

    int[,] Flags { get; set; }

    short[,] MapZoneIds { get; set; }
    List<Point2I> ZoneCenters { get; set; }
    List<ConnectedPairData> ZoneConnections { get; set; }
    float[,] MaintainHeights { get; set; }
    float[,] NearestMountainTopHeight { get; set; }
    float[,] MountainCenterDist { get; set; }
    float[,] MountainDistPercent { get; set; }
    float[,] EdgeMountainDistPercent { get; set; }


    bool SetEntityData(long x, long z, long entityTypeId, long entityId);

    bool CellHasObject(long x, long z);

    byte[,] EntityTypeIds { get; set; }

    byte[,] EntityIds { get; set; }
    List<int[]> wallEndpoints { get; set; }

    Dictionary<int, List<int>> zoneAdjacencies { get; set; }
    List<GenZone> GenZones { get; set; }

    // Have we copied the heightmap data into the TerrainData?
    bool HaveSetHeights { get; set; }
    // Have we copied the splatmaps data into the TerrainData?
    bool HaveSetAlphaSplats { get; set; }

    bool GeneratingMap { get; set; }

    Dictionary<long, List<long>> zoneTreeIds { get; set; }
    Dictionary<long, List<long>> zoneBushIds { get; set; }

    void ClearGenerationData();


    void ClearAlphasAt(int x, int z);

    GenZone GetGenZone(long zoneId);

    float GetAverageHeightNear(Map map, int hx, int hz, int radius, int terrainType = -1);

    float GetAverageSplatNear(int x, int z, int radius, int channel);

    float EdgeHeightmapAdjustPercent(Map map, int x, int z);
    void AddMapLocation(IMapProvider _mapProvider, Location loc);
    float GetMountainDefaultSize(Map map);
}


public class MapGenData : IMapGenData
{

    // Alphamap width and height
    public int Awid { get; set; }
    public int Ahgt { get; set; }

    // Detail map width and height
    public int DWid { get; set; }
    public int DHgt { get; set; }

    public byte[,,] GrassAmounts { get; set; }

    // heightmap
    public float[,] Heights { get; set; }
    public float[,] SubZonePercents { get; set; }
    public int[,] SubZoneIds { get; set; }
    public float[,] OverrideZoneScales { get; set; }
    // splatmaps
    public float[,,] Alphas { get; set; }

    public float[,] RoadDistances { get; set; }


    // List of roads created
    public List<List<LineCell>> Roads { get; set; }

    public List<Point2I> CreviceBridges { get; set; }
    // Bridges that have been made
    public List<Point2I> CurrBridges { get; set; }

    public ushort[,] BridgeDistances { get; set; }


    public float[,] MountainNoise { get; set; }
    public float[,] MountainDecayPower { get; set; }

    public List<Location>[,] LocationGrid { get; set; }

    // Ends of ramps where special monsters or quests can be placed.
    public List<Point2F> RampTops { get; set; }

    public float[,] CreviceDepths { get; set; }

    public int[,] Flags { get; set; }

    public short[,] MapZoneIds { get; set; }
    public List<Point2I> ZoneCenters { get; set; }
    public List<ConnectedPairData> ZoneConnections { get; set; }
    public float[,] MaintainHeights { get; set; }
    public float[,] NearestMountainTopHeight { get; set; }
    public float[,] MountainCenterDist { get; set; }
    public float[,] MountainDistPercent { get; set; }
    public float[,] EdgeMountainDistPercent { get; set; }

    public ExtendedWorldObjectData[,] ExtendedObjects { get; set; }



    public bool SetEntityData(long x, long z, long entityTypeId, long entityId)
    {

        if (CellHasObject(x, z))
        {
            return false;
        }

        EntityTypeIds[x, z] = (byte)entityTypeId;
        EntityIds[x, z] = (byte)entityId;
        return true;
    }



    public bool CellHasObject(long x, long z)
    {
        if (x < 0 || x >= EntityTypeIds.GetLength(0)
           ||
           z < 0 || z >= EntityTypeIds.GetLength(1))
        {
            return true;
        }

        if (EntityTypeIds[x, z] == 0 || EntityTypeIds[x, z] == EntityTypes.Plant)
        {
            return false;
        }

        return true;
    }


    public byte[,] EntityTypeIds { get; set; }

    public byte[,] EntityIds { get; set; }
    public List<int[]> wallEndpoints { get; set; }


    public Dictionary<int, List<int>> zoneAdjacencies { get; set; } = new Dictionary<int, List<int>>();



    public List<GenZone> GenZones { get; set; } = new List<GenZone>();



    // Have we copied the heightmap data into the TerrainData?
    public bool HaveSetHeights { get; set; } = false;
    // Have we copied the splatmaps data into the TerrainData?
    public bool HaveSetAlphaSplats { get; set; } = false;

    public bool GeneratingMap { get; set; } = false;

    public Dictionary<long, List<long>> zoneTreeIds { get; set; } = null;
    public Dictionary<long, List<long>> zoneBushIds { get; set; } = null;

    public virtual void ClearGenerationData()
    {

        GrassAmounts = null;
        Heights = null;
        Alphas = null;
        RoadDistances = null;
        Roads = null;
        CreviceBridges = null;
        CurrBridges = null;
        BridgeDistances = null;
        LocationGrid = null;
        RampTops = null;
        CreviceDepths = null;
        Flags = null;
        MapZoneIds = null;
        ZoneCenters = null;
        MaintainHeights = null;
        MountainDistPercent = null;
        EdgeMountainDistPercent = null;
        wallEndpoints = null;
        EntityTypeIds = null;
        EntityIds = null;
        zoneTreeIds = null;
        zoneBushIds = null;
        ExtendedObjects = null;
    }

    public MapGenData()
    {
    }






    public void ClearAlphasAt(int x, int z)
    {
        if (x < 0 || z < 0 || x >= Awid || z >= Ahgt)
        {
            return;
        }

        for (int c = 0; c < TerrainTexChannels.Max; c++)
        {
            Alphas[x, z, c] *= 0;
        }
    }

    public GenZone GetGenZone(long zoneId)
    {
        GenZone genZone = GenZones.FirstOrDefault(x => x.IdKey == zoneId);
        if (genZone == null)
        {
            genZone = new GenZone() { IdKey = zoneId };
            GenZones.Add(genZone);
        }
        return genZone;
    }

    public float GetAverageHeightNear(Map map, int hx, int hz, int radius, int terrainType = -1)
    {
        if (Heights == null)
        {
            return -1;
        }

        if (HaveSetHeights)
        {
            throw new Exception("You have already set the heights in the heightmap");
        }

        if (radius < 0)
        {
            radius = 0;
        }

        float totalHeight = 0;
        int totalCells = 0;

        for (int x = hx - radius; x <= hx + radius; x++)
        {
            if (x < 0 || x >= map.GetHwid() || x >= Awid)
            {
                continue;
            }
            for (int z = hz - radius; z <= hz + radius; z++)
            {
                if (z < 0 || z >= map.GetHhgt() || z >= map.GetHhgt())
                {
                    continue;
                }
                if (terrainType < 0 || terrainType < Alphas.Length && Alphas[x, z, terrainType] > 0)
                {
                    totalHeight += Heights[x, z];
                    totalCells++;
                }
            }
        }

        if (totalCells < 1)
        {
            return -1;
        }
        return totalHeight / totalCells;

    }

    public float GetAverageSplatNear(int x, int z, int radius, int channel)
    {
        if (Alphas == null || radius < 1 || channel < 0 || channel >= TerrainTexChannels.Max)
        {
            return 0.0f;
        }

        float[,,] alphas2 = Alphas;
        int minx = Math.Max(0, x - radius);
        int maxx = Math.Min(alphas2.GetLength(0) - 1, x + radius);
        int minz = Math.Max(0, z - radius);
        int maxz = Math.Min(alphas2.GetLength(1) - 1, z + radius);

        float totalDirt = 0.0f;
        int cellCount = 0;

        for (int xx = minx; xx <= maxx; xx++)
        {
            for (int zz = minz; zz <= maxz; zz++)
            {
                totalDirt += alphas2[xx, zz, channel];
                cellCount++;

            }
        }

        if (cellCount < 1)
        {
            return 0.0f;
        }

        return totalDirt / cellCount;
    }

    public float EdgeHeightmapAdjustPercent(Map map, int x, int z)
    {
        if (x < 0 || z < 0 || x >= map.GetHwid() || z > map.GetHhgt())
        {
            return 0.0f;
        }

        int edgeSize = MapConstants.MapEdgeSize;
        if (edgeSize > map.GetHwid() / 2)
        {
            edgeSize = map.GetHwid() / 2;
        }

        if (x > edgeSize && x < map.GetHwid() - edgeSize && z > edgeSize && z < map.GetHhgt() - edgeSize)
        {
            return 1.0f;
        }



        int minDistX = Math.Min(x, map.GetHwid() - x);
        int minDistY = Math.Min(z, map.GetHhgt() - z);
        int minDist = Math.Min(minDistX, minDistY);

        return 1.0f * minDist / edgeSize;
    }

    protected int locationCount = 0;
    public void AddMapLocation(IMapProvider _mapProvider, Location loc)
    {
        if (loc == null)
        {
            return;
        }
        if (LocationGrid == null)
        {
            LocationGrid = new List<Location>[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];
            for (int x = 0; x < MapConstants.MaxTerrainGridSize; x++)
            {
                for (int z = 0; z < MapConstants.MaxTerrainGridSize; z++)
                {
                    LocationGrid[x, z] = new List<Location>();
                }
            }
        }

        if (loc.CenterX < 0 || loc.CenterX >= _mapProvider.GetMap().GetHwid() ||
            loc.CenterZ < 0 || loc.CenterZ >= _mapProvider.GetMap().GetHhgt())
        {
            return;
        }

        Zone zone = _mapProvider.GetMap().Get<Zone>(MapZoneIds[loc.CenterX, loc.CenterZ]);
        if (zone != null)
        {
            zone.Locations.Add(loc);

            int gx = MathUtil.Clamp(0, loc.CenterX / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);
            int gz = MathUtil.Clamp(0, loc.CenterZ / MapConstants.TerrainPatchSize, MapConstants.MaxTerrainGridSize - 1);

            LocationGrid[gx, gz].Add(loc);

            loc.Id = _mapProvider.GetMap().Id + "-" + (++locationCount);
        }
    }

    public float GetMountainDefaultSize(Map map)
    {
        if (map == null)
        {
            return 30;
        }

        float zoneSize = map.ZoneSize * MapConstants.TerrainPatchSize;
        return MathUtil.Clamp(MapConstants.MinMountainWidth, zoneSize / MapConstants.MountainWidthDivisor, MapConstants.MaxMountainWidth);
    }

}


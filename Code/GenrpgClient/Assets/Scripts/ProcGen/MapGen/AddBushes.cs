
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Settings.Locations;
using OxDb.SharedGame.ProcGen.Settings.Trees;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;


// Bushs are now not a part of the terrain system.

internal class FullBushPrototype
{
    public string Name = "";
    public BushType treeType { get; set; }
    public IDictionary<string, OverrideBushType> overrideBushTypes { get; set; }
    public ZoneBushType zoneBush = null;
    public ZoneBushType zoneTypeBush = null;
    public int prototypeIndex = 0;
    public MyRandom posRand;
    public MyRandom chanceRand;
    public MyRandom bareRand;
    public double chanceMult;
    public float overrideChance;

    public double currChance = 0.0f;

    public FullBushPrototype()
    {
        overrideBushTypes = new Dictionary<string, OverrideBushType>();
    }

}

internal class OverrideBushType
{
    public float chance;
    public BushType bushType;
}


internal class BushCategory
{
    public int Index;
    public string Name;
    public List<FullBushPrototype> list;
    public int numItems;
    public float freqMult = 1.0f;
    public float densityMult = 1.0f;
    public float posDeltaScale = 1.0f;
    public float overrideChance = 0.0f;
    public float skipChance = 0.0f;
    public int Count;

    public BushCategory()
    {
        list = new List<FullBushPrototype>();
    }
}

internal class ZoneBushData
{
    public ZoneType zoneType;
    public Zone zone;
    public List<BushCategory> categories;

    public BushCategory GetCategory(int index)
    {
        if (categories == null)
        {
            return null;
        }

        return categories.FirstOrDefault(x => x.Index == index);
    }
}

public class AddBushes : BaseZoneGenerator
{
    private IAddNearbyItemsHelper _addNearbyItemsHelper;

    public const int BushIndex = 1;
    public const int WaterIndex = 2;

    public const int BushPlacementSkipSize = 6;
    public const int WaterItemPlacementSkipSize = 4;
    public const float WaterChance = 0.65f;
    public const float BushUniformChance = 0.02f;
    public const float BushNoiseChance = 0.02f;
    public const float MinWallBushChance = 0.35f;
    public const float BushSizeScale = 1.5f;
    public const float MaxBushChance = 0.02f;

    private string[] _treeOverrideNames = new String[] { "Fall", "Young", "Bare", "FallYoung" };

    private float[,] extraBushHeights;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        Dictionary<long, ZoneBushData> ztdict = new Dictionary<long, ZoneBushData>();

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            ZoneBushData ndata = CreateZoneBushData(zone);

            if (ndata != null)
            {
                ztdict[ndata.zone.IdKey] = ndata;
            }
        }

        extraBushHeights = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

        AddBushListsToMap(ztdict);

        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {
                _md.Heights[x, z] += extraBushHeights[x, z];
            }
        }

        foreach (ZoneBushData ztd in ztdict.Values)
        {
            foreach (BushCategory tcat in ztd.categories)
            {
                if (tcat.Count > 0)
                {
                    _logService.Info(tcat.Count + " " + tcat.Name + " Placed in " + ztd.zone.Name + ": [" + ztd.zone.IdKey + "]");
                }
            }
        }

        _zoneGenService.SetAllHeightmaps(_md.Heights, token);
    }

    private ZoneBushData CreateZoneBushData(Zone zone)
    {
        ZoneBushData treeData = new ZoneBushData();

        if (zone == null)
        {
            return null;
        }

        ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId);
        if (zoneType == null)
        {
            return null;
        }

        treeData.zone = zone;
        treeData.zoneType = zoneType;

        MyRandom choiceRand = new MyRandom(zone.Seed + 3241 + zone.IdKey * 13 + zone.ZoneTypeId * 5);

        // Have categories of items to make it easier ot manage different sets of items.
        treeData.categories = SetupBushCategories(zone, zoneType, choiceRand);

        GenZone genZone = _md.GetGenZone(zone.IdKey);

        if (genZone.BushTypes == null)
        {
            return null;
        }

        List<ZoneBushType> tlist = new List<ZoneBushType>(genZone.BushTypes);


        // Get valid list of trees and set up some
        // objects so we can modify values later on.
        for (int t = 0; t < tlist.Count; t++)
        {

            ZoneBushType zoneBush = tlist[t];
            BushType treeType = _gameData.Get<BushTypeSettings>(_gs.ch).Get(zoneBush.BushTypeId);


            if (treeType == null || string.IsNullOrEmpty(treeType.Art))
            {
                continue;
            }

            ZoneBushType zoneTypeBush = null;

            if (genZone.BushTypes != null)
            {
                zoneTypeBush = genZone.BushTypes.FirstOrDefault(x => x.BushTypeId == zoneBush.BushTypeId);
            }

            // If we fail to find the proper tree, make it appear at a lower percent
            // chance.
            if (zoneTypeBush == null)
            {
                continue;
            }



            FullBushPrototype full = new FullBushPrototype();
            full.zoneBush = zoneBush;
            full.zoneTypeBush = zoneTypeBush;
            full.treeType = treeType;
            full.Name = full.treeType.Name;
            full.posRand = new MyRandom(zone.Seed + treeType.IdKey * 23423 + 324);
            full.prototypeIndex = t;
            full.chanceRand = new MyRandom(zone.Seed + treeType.IdKey * 23 + 43535);
            full.bareRand = new MyRandom(zone.Seed % 23423243 + treeType.IdKey * 234231);
            full.overrideChance = RandUtils.FloatRange(MapConstants.MaxOverrideTreeTypeChance / 2,
                MapConstants.MaxOverrideTreeTypeChance, choiceRand);
            full.chanceMult = zoneBush.Weight * zoneTypeBush.Weight;

            if (choiceRand.NextDouble() < 0.35f)
            {
                full.overrideChance *= RandUtils.FloatRange(0.4f, 4.0f, choiceRand);
            }
            if (choiceRand.NextDouble() < 0.35f)
            {
                full.chanceMult *= RandUtils.FloatRange(0.5f, 5.0f, choiceRand);
            }
            SetupBushTypeOverrides(full, treeType);

            if (full.Name == null)
            {
                full.Name = "Bush";
            }

            int categoryIndex = BushIndex;
            if (full.treeType.HasFlag(BushFlags.IsWaterItem))
            {
                categoryIndex = WaterIndex;
            }
            else
            {
                categoryIndex = BushIndex;
            }

            BushCategory tc = treeData.GetCategory(categoryIndex);
            tc.list.Add(full);
        }

        foreach (BushCategory tc in treeData.categories)
        {
            if (tc.list == null)
            {
                continue;
            }

            while (tc.list.Count > tc.numItems)
            {
                tc.list.RemoveAt(choiceRand.Next() % tc.list.Count);
            }
        }

        return treeData;
    }

    private void AddBushListsToMap(Dictionary<long, ZoneBushData> treeData)
    {
        AddBushCategoryToMap(BushIndex, treeData, BushPlacementSkipSize);
        AddBushCategoryToMap(BushIndex, treeData, BushPlacementSkipSize);
        AddBushCategoryToMap(WaterIndex, treeData, WaterItemPlacementSkipSize);
    }


    private void AddBushCategoryToMap(int listIndex, Dictionary<long, ZoneBushData> treeData, int startSkipSize)
    {
        if (treeData == null)
        {
            return;
        }

        MyRandom choiceRand = new MyRandom(_mapProvider.GetMap().Seed % 1234000000 + listIndex * 234654);


        MyRandom chanceRand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + listIndex * 13254);
        MyRandom skipRand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + listIndex * 2341983);
        int skipSize = startSkipSize;

        float finalChance = (listIndex == BushIndex ? MaxBushChance : MaxBushChance);


        float baseFreq = RandUtils.FloatRange(0.07f, 0.6f, choiceRand) * _mapProvider.GetMap().GetHwid() * 0.1f;
        float baseAmp = RandUtils.FloatRange(0.8f, 1.2f, choiceRand) * 0.8f;
        float basePers = RandUtils.FloatRange(0.2f, 0.3f, choiceRand);


        float roadFreq = RandUtils.FloatRange(0.07f, 0.6f, choiceRand) * _mapProvider.GetMap().GetHwid() * 0.2f;
        float roadAmp = RandUtils.FloatRange(5.0f, 10.0f, choiceRand);
        float roadPers = RandUtils.FloatRange(0.1f, 0.3f, choiceRand);

        float[,] roadNoise = _noiseService.Generate(roadPers, roadFreq, roadAmp, 2, _mapProvider.GetMap().Seed % 329832323 + 874332, _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());


        float replaceFreq = RandUtils.FloatRange(0.07f, 0.6f, choiceRand) * _mapProvider.GetMap().GetHwid() * 0.2f;
        float replaceAmp = RandUtils.FloatRange(0.8f, 1.2f, choiceRand);
        float replacePers = RandUtils.FloatRange(0.4f, 0.7f, choiceRand);

        float[,] replaceNoise = _noiseService.Generate(replacePers, replaceFreq, replaceAmp, 2, _mapProvider.GetMap().Seed % 214423231 + 132, _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());


        List<float[,]> allNoises = new List<float[,]>();

        int numNoise = RandUtils.IntRange(4, 11, choiceRand);
        for (int i = 0; i < numNoise; i++)
        {
            float freq = RandUtils.FloatRange(0.02f, 0.066f, choiceRand) * _mapProvider.GetMap().GetHwid();
            float amp = RandUtils.FloatRange(0.2f, 0.6f, choiceRand) * 1.5f;
            int octaves = 2;
            float pers = RandUtils.FloatRange(0.1f, 0.5f, choiceRand);
            float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, _mapProvider.GetMap().Seed % 23432433 + i * 17, _mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt());
            allNoises.Add(noise);
        }

        int minRoadDist = 6;
        int startRoadDist = 10;

        int skipRadius = skipSize * 3 / 2;
        bool isWaterItem = (listIndex == WaterIndex);
        bool isBush = (listIndex == BushIndex || listIndex == WaterIndex);

        for (int cx = 0; cx < _mapProvider.GetMap().GetHwid(); cx += skipSize)
        {
            for (int cz = 0; cz < _mapProvider.GetMap().GetHhgt(); cz += skipSize)
            {
                int x = cx + RandUtils.IntRange(-skipRadius, skipRadius, skipRand);
                x = MathUtil.Clamp(0, x, _mapProvider.GetMap().GetHwid() - 1);
                int ddx = -x / (MapConstants.TerrainPatchSize - 1);
                if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }
                int z = cz + RandUtils.IntRange(-skipRadius, skipRadius, skipRand);
                z = MathUtil.Clamp(0, z, _mapProvider.GetMap().GetHhgt() - 1);
                int ddz = -z / (MapConstants.TerrainPatchSize - 1);
                if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                Location closeLoc = _zoneGenService.FindMapLocation(x, z, 15);

                bool forceBushs = false;

                if (closeLoc != null)
                {
                    if (listIndex == BushIndex)
                    {
                        forceBushs = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (!isWaterItem)
                {
                    if (_md.BridgeDistances[x, z] < 12)
                    {
                        continue;
                    }

                    float currRoadDist = Math.Max(minRoadDist, startRoadDist + roadNoise[x, z]);

                    if (forceBushs)
                    {
                        currRoadDist = 4;
                    }

                    if (_md.RoadDistances[x, z] <= currRoadDist)
                    {
                        continue;
                    }
                }


                int zoneId = _md.MapZoneIds[x, z]; // zoneobject
                bool haveSecondaryZone = false;
                if (_md.SubZoneIds[x, z] > 0)
                {
                    haveSecondaryZone = true;
                    zoneId = _md.SubZoneIds[x, z];
                }


                int zoneRad = 25;
                int numNearbyTries = Math.Min(RandUtils.IntRange(0, 15, choiceRand), RandUtils.IntRange(0, 15, choiceRand));

                if (haveSecondaryZone)
                {
                    zoneRad = 4;
                    numNearbyTries = 0;
                }

                List<int> zonesNearby = new List<int>();

                for (int tries = 0; tries < numNearbyTries; tries++)
                {

                    int nx = x + RandUtils.IntRange(-zoneRad, zoneRad, choiceRand);
                    int nz = z + RandUtils.IntRange(-zoneRad, zoneRad, choiceRand);
                    nx = MathUtil.Clamp(0, nx, _mapProvider.GetMap().GetHwid() - 1);
                    nz = MathUtil.Clamp(0, nz, _mapProvider.GetMap().GetHhgt() - 1);

                    if (_md.MapZoneIds[x, z] != zoneId) // zoneobject
                    {
                        zonesNearby.Add(_md.MapZoneIds[x, z]); // zoneobject
                    }
                }

                if (zonesNearby.Count > 0)
                {
                    int index = choiceRand.Next() % numNearbyTries;
                    if (index < zonesNearby.Count)
                    {
                        zoneId = zonesNearby[index];
                    }
                }

                if (closeLoc != null)
                {
                    int checkRadius = 2;
                    bool foundLocationPatch = false;
                    for (int lx = x - checkRadius; lx <= x + checkRadius; lx++)
                    {
                        if (lx < 0 || lx >= _mapProvider.GetMap().GetHwid())
                        {
                            continue;
                        }

                        for (int lz = z - checkRadius; lz <= z + checkRadius; lz++)
                        {
                            if (lz < 0 || lz >= _mapProvider.GetMap().GetHhgt())
                            {
                                continue;
                            }

                            if (FlagUtils.MatchesAnyBits(_md.Flags[lx, lz], MapGenFlags.IsLocationPatch))
                            {
                                foundLocationPatch = true;
                                break;
                            }
                        }
                        if (foundLocationPatch)
                        {
                            break;
                        }
                    }

                    if (foundLocationPatch)
                    {
                        continue;
                    }
                }
                if (FlagUtils.MatchesAnyBits(_md.Flags[x + ddx, z + ddz], MapGenFlags.BelowWater))
                {
                    continue;
                }

                if (FlagUtils.MatchesAnyBits(_md.Flags[x + ddx, z + ddz], MapGenFlags.NearWater) != isWaterItem)
                {
                    continue;
                }

                bool extraMountainChance = false;

                if (listIndex == BushIndex && _md.MaintainHeights[x, z] != 0)
                {
                    extraMountainChance = true;
                }

                double listChanceSum = 0.0f;

                if (!treeData.ContainsKey(zoneId))
                {
                    continue;
                }

                ZoneBushData ztData = treeData[zoneId];

                BushCategory category = ztData.GetCategory(listIndex);
                if (category == null || category.list == null)
                {
                    continue;
                }

                if (chanceRand.NextDouble() < category.skipChance && !isWaterItem && !forceBushs)
                {
                    continue;
                }

                List<FullBushPrototype> list = category.list;

                // Get the current chances.
                for (int i = 0; i < list.Count; i++)
                {
                    if (category.freqMult <= 0)
                    {
                        list[i].currChance = list[i].chanceMult * category.densityMult;
                    }
                    else
                    {
                        float val = 0;

                        for (int j = 0; j < numNoise; j++)
                        {
                            float[,] currNoise = allNoises[j];
                            int xindex = ((x + zoneId * 11) * j) % _mapProvider.GetMap().GetHwid();
                            int zindex = ((z + zoneId * 19) * j) % _mapProvider.GetMap().GetHhgt();
                            val += Math.Max(0, currNoise[xindex, zindex]);
                        }
                        list[i].currChance = Math.Max(0, val) * category.densityMult;
                    }

                    if (extraMountainChance && list[i].currChance < MinWallBushChance)
                    {
                        list[i].currChance = MinWallBushChance;
                    }
                    list[i].currChance *= 1.0f / list.Count;

                    listChanceSum += list[i].currChance;
                }

                List<FullBushPrototype> currList = new List<FullBushPrototype>();

                if (listChanceSum > finalChance)
                {
                    double scaleDownChance = finalChance / listChanceSum;
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].currChance *= scaleDownChance;
                    }
                }

                double chanceChosen = _gs.Rand.NextDouble();

                for (int i = 0; i < list.Count; i++)
                {
                    chanceChosen -= list[i].currChance;

                    if (chanceChosen <= 0)
                    {
                        currList.Add(list[i]);
                        break;
                    }
                }

                if (forceBushs && currList.Count < 1 && list.Count > 0)
                {
                    currList.Add(list[choiceRand.Next() % list.Count]);
                }

                foreach (FullBushPrototype full in currList)
                {
                    AddBushActual(ztData.zone, full, category, x, z, (1 + replaceNoise[x, z]));
                }
            }
        }
    }



    // Currently rocks, bushes and blank (regular trees)
    internal List<BushCategory> SetupBushCategories(Zone zone, ZoneType zoneType, MyRandom choiceRand)
    {
        if (zone == null || zoneType == null)
        {
            return new List<BushCategory>();
        }

        List<BushCategory> list = new List<BushCategory>();

        BushCategory tc = null;

        ZoneType zt = zoneType;

        GenZone genZone = _md.GetGenZone(zone.IdKey);

        tc = new BushCategory();
        tc.Index = BushIndex;
        tc.Name = "Bushs";
        tc.freqMult = genZone.BushFreq * zoneType.BushFreq;
        tc.densityMult = genZone.BushDensity * zoneType.BushDensity;
        tc.numItems = RandUtils.IntRange(2, 4, choiceRand);
        tc.skipChance = (tc.freqMult <= 0 ? 0.15f : 0.75f);
        if (choiceRand.NextDouble() < 0.2f)
        {
            tc.numItems += RandUtils.IntRange(1, 3, choiceRand);
        }
        tc.densityMult *= (tc.freqMult <= 0 ? BushUniformChance : BushNoiseChance);
        list.Add(tc);

        tc = new BushCategory();
        tc.Index = BushIndex;
        tc.Name = "Bushes";
        tc.freqMult = genZone.BushFreq * zoneType.BushFreq * 2;
        tc.densityMult = genZone.BushDensity * zoneType.BushDensity;
        tc.posDeltaScale = 2.0f;
        tc.numItems = RandUtils.IntRange(3, 5, choiceRand);
        tc.densityMult *= (tc.freqMult <= 0 ? BushUniformChance : BushNoiseChance);
        tc.skipChance =
        tc.skipChance = (tc.freqMult <= 0 ? 0.15f : 0.75f);
        if (choiceRand.NextDouble() < 0.1f)
        {
            tc.numItems += RandUtils.IntRange(1, 3, choiceRand);
        }
        list.Add(tc);

        tc = new BushCategory();
        tc.Index = WaterIndex;
        tc.Name = "Water";
        float bushDensity = genZone.BushDensity * zoneType.BushDensity;
        if (bushDensity <= 0)
        {
            bushDensity = 0.1f;
        }

        if (bushDensity < 1.0f)
        {
            bushDensity = (float)Math.Sqrt(bushDensity);
        }
        tc.densityMult = WaterChance * RandUtils.FloatRange(0.4f, 1.6f, choiceRand) * bushDensity;
        tc.freqMult *= 0.0f;
        tc.posDeltaScale = 1.0f;
        tc.skipChance =
        tc.skipChance = (tc.freqMult <= 0 ? 0.15f : 0.75f);
        tc.numItems = RandUtils.IntRange(2, 3, choiceRand);
        list.Add(tc);

        return list;
    }

    private BushType GetFinalBushOverride(BushCategory tcat, FullBushPrototype full, Zone zone, float replaceChanceMult)
    {
        if (zone == null || tcat == null || full == null || full.treeType == null)
        {
            return new BushType();
        }

        if (full.bareRand == null || full.overrideBushTypes == null || full.overrideBushTypes.Keys.Count < 1)
        {
            return full.treeType;
        }

        if (full.bareRand.NextDouble() > full.overrideChance * replaceChanceMult)
        {
            return full.treeType;
        }

        double choiceTotal = 0.0f;

        foreach (OverrideBushType val in full.overrideBushTypes.Values)
        {
            if (val.bushType != null)
            {
                choiceTotal += val.chance;
            }
        }

        double choiceChosen = full.bareRand.NextDouble() * choiceTotal;

        foreach (OverrideBushType val in full.overrideBushTypes.Values)
        {
            if (val.bushType != null)
            {
                choiceChosen -= val.chance;
                if (choiceChosen <= 0)
                {
                    return val.bushType;
                }
            }
        }

        return full.treeType;
    }

    private void SetupBushTypeOverrides(FullBushPrototype full, BushType ttype)
    {
        if (ttype == null || ttype.Art == null || full == null || full.bareRand == null)
        {
            return;
        }

        string tname = ttype.Art;
        tname = tname.Replace("Winter", "");

        foreach (BushType item in _gameData.Get<BushTypeSettings>(_gs.ch).GetData())
        {
            if (item.Art != null && item.Art != ttype.Art)
            {
                foreach (string name in _treeOverrideNames)
                {
                    if (item.Art.Replace(name, "") == tname)
                    {
                        OverrideBushType over = new OverrideBushType()
                        {
                            bushType = item,
                        };
                        over.chance = RandUtils.FloatRange(0, 0.1f, full.bareRand);
                        if (full.bareRand.NextDouble() < 0.1f)
                        {
                            over.chance *= RandUtils.FloatRange(5, 50, full.bareRand);
                        }
                        full.overrideBushTypes[name] = over;
                    }
                }
            }
        }
    }

    private void AddBushActual(
                                Zone zone,
                                FullBushPrototype full,
                                BushCategory tcat,
                                int x, int z, float replaceChanceMult)
    {
        x -= x / (MapConstants.TerrainPatchSize - 1);
        z -= z / (MapConstants.TerrainPatchSize - 1);

        BushType treeType = full.treeType;

        long treeTypeId = 0;
        if (full.treeType != null)
        {
            treeTypeId = full.treeType.IdKey;
        }
        treeType = GetFinalBushOverride(tcat, full, zone, replaceChanceMult);

        if (treeTypeId == 0 || treeType == null)
        {
            return;
        }

        if (x >= 0 && z >= 0 && x < _mapProvider.GetMap().GetHwid() && z < _mapProvider.GetMap().GetHhgt())
        {
            if (_md.Heights[x, z] < MapConstants.OceanHeight / MapConstants.MapHeight)
            {
                return;
            }

            if (!_md.CellHasObject(x, z))
            {
                _md.SetEntityData(x, z, EntityTypes.Bush, treeType.IdKey);
                tcat.Count++;

                float dirtRadius = 1;

                if (!full.treeType.HasFlag(BushFlags.NoNearbyItems))
                {
                    int numNearbyItems = RandUtils.IntRange(2, 9, full.chanceRand);
                    if (full.chanceRand.NextDouble() < 0.3f)
                    {
                        numNearbyItems += RandUtils.IntRange(2, 9, full.chanceRand);
                    }
                    numNearbyItems += 5;


                    float maxRadius = Math.Max(2.0f, dirtRadius / 2);
                    float minRadius = Math.Max(1.0f, maxRadius / 2);

                    _addNearbyItemsHelper.AddItemsNear(full.posRand, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone, x, z, 1.0f, numNearbyItems, minRadius, maxRadius, false);
                }
            }
        }
    }
}





using OxDb.Client.ProcGen.Loading.Utils;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Settings.Plants;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BaseDetailPrototype
{
    public ZonePlantType zonePlant = null;
    public PlantType plantType = null;
    public long noiseSeed = 0;
    public int Index = 0;
    public int XGrid = -1;
    public int ZGrid = -1;
    public List<long> zoneIds = new List<long>();
}

public class AddPlants : BaseZoneGenerator
{
    public const float GrassBaseScale = 1.0f;
    public const float GrassDensityScale = 1.0f;
    public const float GrassRandomChance = 0.005f;

    public const float GrassFreqScale = 1.0f;

    private IZonePlantValidator _zonePlantValidator;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.MinX, zone.MinZ, zone.MaxX, zone.MaxZ);
        }
        AddPlantsToMapData(_gs);
    }





    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int startz, int endx, int endz)
    {
        if (startx >= endx || startz >= endz)
        {
            return;
        }

        List<ZonePlantType> plist = zone.PlantTypes;
        if (plist == null)
        {
            return;
        }

        List<FullDetailPrototype> fullList = new List<FullDetailPrototype>();

        _zonePlantValidator.UpdateValidPlantTypeList(zone, -1, -1, fullList, true, _token);

        if (fullList == null)
        {
            return;
        }

        int dx = endx - startx + 1;
        int dz = endz - startz + 1;
        int perlinSize = Math.Max(MapConstants.DefaultNoiseSize, Math.Max(dx, dz));
        float perlinScale = perlinSize * 1.0f / MapConstants.DefaultHeightmapSize;
        while (fullList.Count > MapConstants.MaxGrass)
        {
            fullList.RemoveAt(fullList.Count - 1);
        }

        for (int index = 0; index < fullList.Count; index++)
        {
            FullDetailPrototype full = fullList[index];

            if (full.noiseSeed == 0 || full.plantType == null || full.zonePlant == null)
            {
                continue;
            }
            long pseed = full.noiseSeed;

            int plantChanceTimes = 2;

            List<float[,]> plantChances = new List<float[,]>();

            float density = 0.0f;

            float midSteepVal = 70f;

            GenZone genZone = _md.GetGenZone(zone.IdKey);

            MyRandom rand = new MyRandom(full.noiseSeed);
            for (int i = 0; i < plantChanceTimes; i++)
            {
                float pers = RandUtils.FloatRange(0.1f, 0.3f, rand) * 1.2f;
                float amp = RandUtils.FloatRange(1.0f, 2.0f, rand) * 0.9f;
                float freq = perlinSize * RandUtils.FloatRange(0.04f, 0.25f, rand);

                int octaves = 2;


                freq *= genZone.GrassFreq * zoneType.GrassFreq * GrassFreqScale;

                freq *= perlinScale;


                if (freq < 8)
                {
                    freq = 8;
                }

                density = genZone.GrassDensity * zoneType.GrassDensity * GrassDensityScale;

                amp *= density;



                plantChances.Add(_noiseService.Generate(pers, freq, amp, octaves, pseed, perlinSize, perlinSize));
            }

            float steepFreq = perlinSize * RandUtils.FloatRange(0.05f, 0.15f, rand);
            float steepAmp = RandUtils.FloatRange(4, 20, rand);
            float steepPers = RandUtils.FloatRange(0.1f, 0.3f, rand);
            int steepOctaves = 2;

            /// Steepness allowed at each coord for this grass to grow or not.
            float[,] steepVals = _noiseService.Generate(steepPers, steepFreq, steepAmp, steepOctaves, pseed + 1, perlinSize, perlinSize);

            int numChecked = 0;
            int badZoneId = 0;
            int nearRoad = 0;
            int nearLocation = 0;
            int didSet = 0;

            bool useUniformDensity = false;
            for (int x = startx; x <= endx; x++)
            {
                for (int z = startz; z <= endz; z++)
                {
                    float currDensityMult = RandUtils.FloatRange(0, 2, rand);
                    numChecked++;

                    if (_md.MapZoneIds[x, z] != zone.IdKey) // zoneobject
                    {
                        badZoneId++;
                        continue;
                    }

                    if (_md.CellHasObject(x, z))
                    {
                        continue;
                    }

                    if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.BelowWater))
                    {
                        continue;
                    }
                    if (_md.Alphas[x, z, TerrainTexChannels.Road] > 0)
                    {
                        bool isNearRoad = false;
                        int roadRad = 0;
                        for (int xx = x - roadRad; xx <= x + roadRad; xx++)
                        {
                            if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                            {
                                continue;
                            }
                            for (int zz = z - roadRad; zz <= z + roadRad; zz++)
                            {
                                if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                                {
                                    continue;
                                }
                                if (_md.Alphas[xx, zz, TerrainTexChannels.Road] > 0)
                                {
                                    isNearRoad = true;
                                    break;
                                }
                            }
                        }
                        if (isNearRoad)
                        {
                            nearRoad++;
                            continue;
                        }
                    }
                    float hgt = _terrainManager.SampleHeight(x, z);
                    if (hgt < MapConstants.MinLandHeight * 7 / 10)
                    {
                        continue;
                    }


                    float steep = _terrainManager.GetSteepness(x, z);

                    if (steep > (midSteepVal + steepVals[x - startx, z - startz]))
                    {
                        continue;
                    }

                    float chance = 0;
                    if (!full.plantType.HasFlag(PlantFlags.SmallPatches) && !useUniformDensity)
                    {

                        for (int i = 0; i < plantChanceTimes; i++)
                        {
                            float origChance = plantChances[i][x - startx, z - startz];
                            if (origChance < 0)
                            {
                                origChance = -origChance / 2;
                            }
                            chance += origChance;
                        }

                        chance /= plantChanceTimes;

                        if (chance > 1)
                        {
                            chance = 1;
                        }
                    }
                    else
                    {
                        if (_gs.Rand.NextDouble() > currDensityMult * density / 20.0f)
                        {
                            continue;
                        }
                        else
                        {
                            chance = RandUtils.FloatRange(0, 1, rand);
                        }
                    }

                    if (_zoneGenService.FindMapLocation(x, z, 1) != null)
                    {
                        if (FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.IsLocationPatch))
                        {
                            nearLocation++;
                            continue;
                        }
                    }


                    short val = (short)(chance * MapConstants.MaxGrassValue);

                    if (val < 1 && rand.NextDouble() < GrassRandomChance)
                    {
                        val = (short)RandUtils.IntRange(1, 3, rand);
                    }

                    if (val > 0)
                    {
                        didSet++;

                        val = (short)Math.Max(steep / 10, val);
                        if (val > MapConstants.MaxGrassValue)
                        {
                            val = MapConstants.MaxGrassValue;
                        }

                        if (val < 3)
                        {
                            val++;
                        }

                        int ny = z - (z / (MapConstants.TerrainPatchSize - 1)) * 0;
                        int nx = x - (x / (MapConstants.TerrainPatchSize - 1)) * 0;
                        if (full.plantType.HasFlag(PlantFlags.UsePrefab))
                        {
                            val = (short)(val * MapConstants.PrefabPlantDensityScale);
                        }
                        _md.GrassAmounts[nx, ny, index] = (byte)val;
                    }
                }

            }
        }
    }

    public void AddPlantsToMapData(IClientGameState gs)
    {

        if (_md.GrassAmounts == null)
        {
            return;
        }
        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {
                if (!_md.CellHasObject(x, z))
                {
                    int val = 0;
                    int[] vals = new int[MapConstants.MaxGrass];
                    for (int i = 0; i < MapConstants.MaxGrass; i++)
                    {
                        int currVal = Math.Min(MapConstants.MaxGrassValue, (int)_md.GrassAmounts[x, z, i]);
                        vals[i] = currVal;
                        for (int j = 0; j < i; j++)
                        {
                            currVal *= (MapConstants.MaxGrassValue + 1);
                        }
                        val += currVal;
                    }
                    if (val != 0)
                    {
                        _md.SetEntityData(x, z, EntityTypes.Plant, val);
                    }
                }
            }
        }
    }
}




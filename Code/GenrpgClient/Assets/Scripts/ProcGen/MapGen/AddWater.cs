using OxDb.Client.ProcGen.Loading.Utils;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.ProcGen.Settings.MapWater;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Threading;
using UnityEngine;

public class AddWater : BaseZoneGenerator
{

    private IAddPoolService _addPoolService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone);
        }
    }

    /// <summary>
    /// Attempt to add Pool(s) to a given zone.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="zone"></param>
    public void GenerateOne(Zone zone)
    {
        if (zone == null)
        {
            return;
        }

        ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId);
        if (ztype == null)
        {
            return;
        }

        int minx = zone.MinX;
        int maxx = zone.MaxX;
        int minz = zone.MinZ;
        int maxz = zone.MaxZ;


        int totalSize = (maxx - minx) * (maxz - minz);

        totalSize /= (MapConstants.TerrainPatchSize * MapConstants.TerrainPatchSize);

        totalSize /= 10;

        float worldBaseHeight = 1.0f * MapConstants.MinLandHeight / MapConstants.MapHeight;

        int numPools = totalSize;

        MyRandom rand = new MyRandom(zone.Seed % 1000000000 + 2438932);


        int currentPools = 0;

        int totalTries = 100 * numPools;

        for (int times = 0; times < totalTries; times++)
        {
            if (currentPools >= numPools)
            {
                break;
            }

            int cx = RandUtils.IntRange(minx, maxx, rand);
            int cz = RandUtils.IntRange(minz, maxz, rand);


            int rad = 100;
            float minDistToFeature = rad * 3 / 2;


            bool onEdgeOfMap = false;

            for (int x = cx - rad; x <= cx + rad; x++)
            {
                if (onEdgeOfMap)
                {
                    break;
                }

                if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                {
                    onEdgeOfMap = true;
                    break;
                }
                int dx = x - cx;
                for (int z = cz - rad; z <= cz + rad; z++)
                {
                    if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                    {
                        onEdgeOfMap = true;
                        break;
                    }
                    int dy = z - cz;

                    if (_md.Alphas[x, z, TerrainTexChannels.Road] > 0 ||
                        _md.MaintainHeights[x, z] != 0 ||
                        FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.IsLocation |
                        MapGenFlags.NearWater))
                    {
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (dist < minDistToFeature)
                        {
                            minDistToFeature = dist;
                        }
                    }
                }
            }

            if (onEdgeOfMap)
            {
                continue;
            }

            int minDist = rad / (1 + (times * 3) / totalTries);

            if (minDistToFeature < minDist)
            {
                continue;
            }

            int maxRadius = (int)(minDistToFeature - 10);


            WaterGenData poolData = new WaterGenData()
            {
                x = cx,
                z = cz,
                minXSize = maxRadius / 2,
                maxXSize = maxRadius,
                minZSize = maxRadius / 2,
                maxZSize = maxRadius,
                stepSize = 1,
            };

            int deformSeed = rand.Next();

            AlterHeightsNear(cx, cz, maxRadius, deformSeed, true);


            if (!_addPoolService.TryAddPool(poolData))
            {
                AlterHeightsNear(cx, cz, maxRadius, deformSeed, false);
            }
            else
            {
                currentPools++;
                for (int x = cx - maxRadius; x <= cx + maxRadius; x++)
                {
                    if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int z = cz - maxRadius; z <= cz + maxRadius; z++)
                    {
                        if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        if (_md.Heights[x, z] < worldBaseHeight &&
                            FlagUtils.MatchesAnyBits(_md.Flags[x, z], MapGenFlags.BelowWater))
                        {
                            _md.Heights[x, z] = worldBaseHeight;
                        }
                    }
                }
            }
        }
    }


    protected void AlterHeightsNear(int cx, int cz, int maxRadius, int randomSeed, bool lowerHeights)
    {
        MyRandom rand = new MyRandom(randomSeed);

        int raiseLowerMult = (lowerHeights ? -1 : 1);

        int size = maxRadius * 2 + 1;

        float heightAmp = RandUtils.FloatRange(0.4f, 0.7f, rand);
        float heightFreq = RandUtils.FloatRange(0.02f, 0.04f, rand) * size;
        float heightPers = RandUtils.FloatRange(0.2f, 0.5f, rand);
        int heightOctaves = 2;
        float[,] heightNoise = _noiseService.Generate(heightPers, heightFreq, heightAmp, heightOctaves, rand.Next(), size, size);

        int maxRadiusX = RandUtils.IntRange(maxRadius * 2 / 3, maxRadius, rand);
        int maxRadiusY = RandUtils.IntRange(maxRadius * 2 / 3, maxRadius, rand);
        int minRadius = Math.Min(maxRadiusX, maxRadiusY);
        float bottomDepth = RandUtils.FloatRange(minRadius / 4, minRadius / 2, rand);


        float radAmp = RandUtils.FloatRange(0.5f, 0.9f, rand);
        float radFreq = RandUtils.FloatRange(5.0f, 14.0f, rand);
        float radPers = RandUtils.FloatRange(0.2f, 0.8f, rand);
        int radOctaves = 2;

        float[,] radNoise = _noiseService.Generate(radPers, radFreq, radAmp, radOctaves, rand.Next(), 360, 360);


        float midPower = RandUtils.FloatRange(0.2f, 0.4f, rand);
        float powerAmp = RandUtils.FloatRange(0.05f, 0.15f, rand);
        float powerFreq = RandUtils.FloatRange(5.0f, 12.0f, rand);
        float powerPers = RandUtils.FloatRange(0.2f, 0.3f, rand);
        int powerOctaves = 2;
        float[,] powerNoise = _noiseService.Generate(powerPers, powerFreq, powerAmp, powerOctaves, rand.Next(), 360, 360);



        int angleRot = rand.Next() % 360;
        int xmin = cx - maxRadiusX;
        int xmax = cx + maxRadiusX;
        int zmin = cz - maxRadiusY;
        int zmax = cz + maxRadiusY;
        for (int xx = xmin; xx <= xmax; xx++)
        {
            if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }

            int dx = xx - cx;
            float xpct = (xx - cx) / (1.0f * maxRadiusX);
            int distToEdgeX = Math.Min(Math.Abs(xx - xmin), Math.Abs(xx - xmax));
            int offsetx = xx - xmin;
            for (int zz = zmin; zz <= zmax; zz++)
            {
                int dz = zz - cz;
                if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                float zpct = (zz - cz) / (1.0f * maxRadiusY);
                int offsetz = zz - zmin;

                int distToEdgeZ = Math.Min(Math.Abs(zz - zmin), Math.Abs(zz - zmax));

                float radMult = 1.0f;

                double angle = Math.Atan2(dx, dz) * 180 / Math.PI + angleRot;

                while (angle >= 360)
                {
                    angle -= 360;
                }

                while (angle < 0)
                {
                    angle += 360;
                }

                int intAngle = (int)(angle);

                float radDelta = radNoise[intAngle, intAngle / 2];

                int distToEnd = Math.Abs(Math.Min(intAngle, 360 - intAngle));

                int distToEndCheck = 5;
                if (distToEnd <= distToEndCheck)
                {
                    radDelta *= 1.0f * distToEnd / distToEndCheck;
                }

                if (radDelta < 0)
                {
                    // If delta < 0 set it to at least -0.5f, then cube to shrink it.
                    radDelta = -(float)(Math.Pow(Math.Abs(radDelta), 3.0f));
                    if (radDelta < -0.5f)
                    {
                        radDelta = -0.5f;
                    }
                    // Negative rad = wider area so we bump up against the edge of the region we are modifying.
                }
                radMult = 0.9f + radDelta;

                float powerDelta = powerNoise[intAngle, intAngle / 2];

                if (distToEnd <= distToEndCheck)
                {
                    powerDelta *= 1.0f * distToEnd / distToEndCheck;
                }

                float depthPower = midPower + powerDelta;

                float pctToEdge = (float)(Math.Pow((xpct * xpct + zpct * zpct) * radMult, depthPower));

                if (pctToEdge > 1)
                {
                    continue;
                }

                float currNoise = heightNoise[offsetx, offsetz];

                float heightDiff = (1 - pctToEdge) * bottomDepth;
                heightDiff += (float)((currNoise * bottomDepth * (1 - pctToEdge * pctToEdge)));

                int edgeScaleDist = 8;

                float edgeScaleDown = Math.Min(distToEdgeX, edgeScaleDist) * 1.0f / edgeScaleDist;
                edgeScaleDown *= Math.Min(distToEdgeZ, edgeScaleDist) * 1.0f / edgeScaleDist;
                edgeScaleDown = (float)(Math.Pow(edgeScaleDown, 0.8f));

                heightDiff *= edgeScaleDown;

                heightDiff /= MapConstants.MapHeight;

                _md.Heights[xx, zz] += heightDiff * raiseLowerMult;
            }
        }
    }

}



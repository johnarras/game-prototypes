
using OxDb.SharedGame.ProcGen.Constants;
using System;

using System.Threading;
using UnityEngine;

public class SmoothHeightsFinal : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        int hwid = _mapProvider.GetMap().GetHwid();
        int hhgt = _mapProvider.GetMap().GetHhgt();

        float[,] heights2 = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];


        int minRadius = 2;
        float smoothScale = 0.5f;
        int checkRadius = 8;


        for (int x = 0; x < hwid; x++)
        {
            for (int z = 0; z < hhgt; z++)
            {
                heights2[x, z] = _md.Heights[x, z];
            }
        }


        for (int x = 0; x < hwid; x++)
        {
            for (int z = 0; z < hhgt; z++)
            {
                int currRadius = minRadius;
                int numLineCellsChecked = 0;
                float totalRoadPercent = 0;
                float currSmoothingScale = smoothScale;
                float otherZoneDist = 10000;


                for (int xx = x - checkRadius; xx <= x + checkRadius; xx++)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int zz = z - checkRadius; zz <= z + checkRadius; zz++)
                    {
                        if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }
                        numLineCellsChecked++;
                        totalRoadPercent += _md.Alphas[xx, zz, TerrainTexChannels.Road];
                        if (_md.MapZoneIds[xx, zz] != _md.MapZoneIds[x, z])
                        {
                            float dx = xx - x;
                            float dz = zz - z;

                            float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                            if (dist < otherZoneDist)
                            {
                                otherZoneDist = dist;
                            }
                        }
                    }
                }

                float bridgeDist = 100;

                if (_md.BridgeDistances != null)
                {
                    bridgeDist = _md.BridgeDistances[x, z];
                }

                float bridgeScale = 1.0f;

                int minBridgeDist = 40;
                if (bridgeDist < minBridgeDist)
                {
                    float mainPct = 0.2f;
                    bridgeScale = mainPct + (1 - mainPct) * bridgeDist / (1.0f * minBridgeDist);
                    currSmoothingScale *= bridgeScale;
                }

                if (totalRoadPercent > 0 && false)
                {
                    float adjustedRoadPercent = Math.Min(1, 1.5f * totalRoadPercent / numLineCellsChecked);
                    currSmoothingScale *= (1 - adjustedRoadPercent);
                }

                else if (otherZoneDist < checkRadius)
                {
                    float newSmoothingScale = (checkRadius - otherZoneDist + 1) / checkRadius;
                    if (newSmoothingScale > 1)
                    {
                        newSmoothingScale = 1;
                    }

                    if (newSmoothingScale > currSmoothingScale)
                    {
                        currSmoothingScale = newSmoothingScale;
                    }
                }

                float totalWeight = 0;
                float totalVal = 0;

                for (int xx = x - currRadius; xx <= x + currRadius; xx++)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    float dx = Math.Abs(xx - x);
                    for (int zz = z - currRadius; zz <= z + currRadius; zz++)
                    {
                        if (zz < 0 || zz >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        float dz = Math.Abs(zz - z);

                        float totalOffset = dx + dz;

                        float currweight = 1;
                        currweight = (float)Math.Pow(currSmoothingScale, totalOffset);

                        totalVal += _md.Heights[xx, zz] * currweight;
                        totalWeight += currweight;
                    }
                }

                if (totalWeight <= 0)
                {
                    continue;
                }

                heights2[x, z] = totalVal / totalWeight;
            }

        }
        _md.Heights = heights2;
    }
}




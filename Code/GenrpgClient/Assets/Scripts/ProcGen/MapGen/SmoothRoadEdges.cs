using OxDb.SharedGame.ProcGen.Constants;
using System;

using System.Threading;
using UnityEngine;

public class SmoothRoadEdges : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);

        int awid = _md.Awid;
        int ahgt = _md.Ahgt;
        int hwid = _mapProvider.GetMap().GetHwid();
        int hhgt = _mapProvider.GetMap().GetHhgt();

        float[,] heights2 = new float[_mapProvider.GetMap().GetHwid(), _mapProvider.GetMap().GetHhgt()];

        int radius = 7;

        radius = radius * 3 / 2;

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
                int ax = (int)(1.0f * x / hwid * awid);
                int az = (int)(1.0f * z / hhgt * ahgt);

                float currSplat = _md.Alphas[ax, az, TerrainTexChannels.Road];
                if (currSplat > 0.0f)
                {
                    //continue;
                }

                if (_md.RoadDistances[ax, az] >= radius)
                {
                    continue;
                }

                float aveSplat = _md.GetAverageSplatNear(ax, az, radius, TerrainTexChannels.Road);

                int averad = radius;
                if (currSplat > 0)
                {
                    averad = 2;
                }
                float aveHeight = _md.GetAverageHeightNear(_mapProvider.GetMap(), x, z, averad);

                if (currSplat <= 0 && _zoneGenService.FindMapLocation(x, z, 5) != null)
                {
                    continue;
                }

                float alterPercent = Math.Max(0.1f, 1 - aveSplat * 3.0f);

                alterPercent = Math.Min(1.0f, aveSplat * 5.0f);

                if (averad < 0)
                {
                    alterPercent /= 3;
                }
                if (currSplat > 0)
                {
                    alterPercent = 1.0f;
                }

                float bridgeDist = _md.BridgeDistances[z, x];

                float bridgeScale = 1.0f;

                int minBridgeDist = 15;
                if (bridgeDist < minBridgeDist)
                {
                    bridgeScale = 0.5f + bridgeDist / (2 * minBridgeDist);
                    alterPercent *= bridgeScale;
                }

                if (alterPercent <= 0)
                {
                    continue;
                }

                float currHeight = _md.Heights[x, z];

                if (aveHeight < currHeight)
                {
                    aveHeight += (currHeight - aveHeight) / 2;
                }

                heights2[x, z] = currHeight + (aveHeight - currHeight) * alterPercent;

            }
        }

        _md.Heights = heights2;
    }
}




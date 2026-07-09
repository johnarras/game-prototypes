
using OxDb.SharedGame.ProcGen.Constants;
using System;
using System.Threading;
using UnityEngine;

public class SmoothTerrainTexturesFinal : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        int awid = _md.Awid;
        int ahgt = _md.Ahgt;

        int radius = 1;
        float smoothScale = 0.04f;

        float[,,] alphas2 = new float[awid, ahgt, TerrainTexChannels.Max];

        for (int x = 0; x < awid; x++)
        {
            for (int z = 0; z < ahgt; z++)
            {
                for (int i = 0; i < TerrainTexChannels.Max; i++)
                {
                    alphas2[x, z, i] = _md.Alphas[x, z, i];
                }
            }
        }

        for (int x = 0; x < awid; x++)
        {
            for (int z = 0; z < ahgt; z++)
            {
                for (int i = 0; i < TerrainTexChannels.Max; i++)
                {
                    float totalWeight = 0;
                    float totalVal = 0;
                    for (int xx = x - radius; xx <= x + radius; xx++)
                    {
                        if (xx < 0 || xx >= _md.Awid)
                        {
                            continue;
                        }

                        int dx = Math.Abs(xx - x);
                        for (int zz = z - radius; zz <= z + radius; zz++)
                        {
                            if (zz < 0 || zz >= _md.Ahgt)
                            {
                                continue;
                            }

                            int dz = Math.Abs(zz - z);
                            int dist = dx + dz;
                            float currWeight = (float)Math.Pow(smoothScale, dist);
                            totalWeight += currWeight;
                            totalVal += _md.Alphas[xx, zz, i] * currWeight;
                        }
                    }
                    alphas2[x, z, i] = totalVal / totalWeight;
                }
            }
        }

        for (int x = 0; x < _md.Awid; x++)
        {
            for (int z = 0; z < _md.Ahgt; z++)
            {
                float total = 0;
                for (int i = 0; i < TerrainTexChannels.Max; i++)
                {
                    total += alphas2[x, z, i];
                }
                for (int i = 0; i < TerrainTexChannels.Max; i++)
                {
                    alphas2[x, z, i] /= total;
                }
            }
        }

        _md.Alphas = alphas2;
    }
}


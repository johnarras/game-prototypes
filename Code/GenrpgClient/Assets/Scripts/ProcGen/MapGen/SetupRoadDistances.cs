
using OxDb.SharedGame.ProcGen.Constants;
using System;
using System.Threading;
using UnityEngine;

public class SetupRoadDistances : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        _md.RoadDistances = new float[_md.Awid, _md.Ahgt];

        for (int x = 0; x < _md.Awid; x++)
        {
            for (int z = 0; z < _md.Ahgt; z++)
            {
                _md.RoadDistances[x, z] = MapConstants.InitialRoadDistance;
            }
        }

        for (int x = 0; x < _md.Awid; x++)
        {
            for (int z = 0; z < _md.Ahgt; z++)
            {
                if (_md.Alphas[x, z, TerrainTexChannels.Road] == 0)
                {
                    continue;
                }

                if (_md.Alphas[x, z, TerrainTexChannels.Road] >= 0.5f)
                {
                    _md.SubZonePercents[x, z] = 0;
                }
                for (int xx = x - MapConstants.MaxRoadCheckDistance; xx <= x + MapConstants.MaxRoadCheckDistance; xx++)
                {
                    if (xx < 0 || xx >= _md.Awid)
                    {
                        continue;
                    }
                    for (int zz = z - MapConstants.MaxRoadCheckDistance; zz <= z + MapConstants.MaxRoadCheckDistance; zz++)
                    {
                        if (zz < 0 || zz >= _md.Ahgt)
                        {
                            continue;
                        }

                        double dist = (ushort)Math.Sqrt((xx - x) * (xx - x) + (zz - z) * (zz - z));
                        if (dist < _md.RoadDistances[xx, zz])
                        {
                            _md.RoadDistances[xx, zz] = (float)dist;
                        }
                    }
                }
            }
        }
    }
}




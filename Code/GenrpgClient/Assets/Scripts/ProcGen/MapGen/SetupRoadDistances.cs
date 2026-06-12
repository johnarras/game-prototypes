
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
            for (int y = 0; y < _md.Ahgt; y++)
            {
                _md.RoadDistances[x, y] = MapConstants.InitialRoadDistance;
            }
        }

        for (int x = 0; x < _md.Awid; x++)
        {
            for (int y = 0; y < _md.Ahgt; y++)
            {
                if (_md.Alphas[x, y, TerrainTexChannels.Road] == 0)
                {
                    continue;
                }

                if (_md.Alphas[x, y, TerrainTexChannels.Road] >= 0.5f)
                {
                    _md.SubZonePercents[x, y] = 0;
                }
                for (int xx = x - MapConstants.MaxRoadCheckDistance; xx <= x + MapConstants.MaxRoadCheckDistance; xx++)
                {
                    if (xx < 0 || xx >= _md.Awid)
                    {
                        continue;
                    }
                    for (int yy = y - MapConstants.MaxRoadCheckDistance; yy <= y + MapConstants.MaxRoadCheckDistance; yy++)
                    {
                        if (yy < 0 || yy >= _md.Ahgt)
                        {
                            continue;
                        }

                        double dist = (ushort)Math.Sqrt((xx - x) * (xx - x) + (yy - y) * (yy - y));
                        if (dist < _md.RoadDistances[xx, yy])
                        {
                            _md.RoadDistances[xx, yy] = (float)dist;
                        }
                    }
                }
            }
        }
    }
}




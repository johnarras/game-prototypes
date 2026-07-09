
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class AddZoneCenters : BaseZoneGenerator
{
    protected ISamplingService _sampleService = null;

    public const int WallPatchId = 1;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        SamplingData sdata = new SamplingData();
        _md.ZoneCenters = new List<Point2I>();
        float edgeSize = MapConstants.TerrainPatchSize * 3 / 4;

        float blockSize = MapConstants.TerrainPatchSize;

        blockSize = _mapProvider.GetMap().ZoneSize * MapConstants.TerrainPatchSize;

        int totalSize = _mapProvider.GetMap().GetHwid();
        float searchSize = _mapProvider.GetMap().GetHwid() - edgeSize;

        if (searchSize < totalSize / 2)
        {
            searchSize = totalSize / 2;
        }

        sdata.Count = (int)((0.45f * totalSize * totalSize) / (blockSize * blockSize));
        if (sdata.Count < 1)
        {
            sdata.Count = 1;

        }

        if (_mapProvider.GetMap().IsSingleZone())
        {
            sdata.Count = 1;
        }

        _logService.Info("Map TotalSize: " + totalSize + " SearchSize: " + searchSize + " BlockSize: " + blockSize);

        sdata.MaxAttemptsPerItem = 1000;
        sdata.MinSeparation = blockSize * 12 / 10;



        sdata.MinX = -(int)(blockSize * 2);
        sdata.MaxX = (int)(_mapProvider.GetMap().GetHwid() + blockSize * 2);
        sdata.MinZ = -(int)(blockSize * 2);
        sdata.MaxZ = (int)(_mapProvider.GetMap().GetHhgt() + blockSize * 2);
        sdata.Seed = _mapProvider.GetMap().Seed % 1000000000 + 3824821;

        sdata.NoiseAmp = RandUtils.FloatRange(0.3f, 0.8f, _gs.Rand);
        sdata.NoiseFreq = RandUtils.FloatRange(3.0f, 10.0f, _gs.Rand);

        SamplingResult result = _sampleService.PlanePoissonSample(sdata);

        List<Point2I> centers = result.Points.Cast<Point2I>().ToList();

        centers = centers.Where(p =>
        p.X >= edgeSize
        && p.Z >= edgeSize
        && p.X <= _mapProvider.GetMap().GetHwid() - edgeSize
        && p.Z <= _mapProvider.GetMap().GetHhgt() - edgeSize).ToList();

        _logService.Info("Centers Wanted: " + sdata.Count + " Found: " + centers.Count);

        if (centers.Count < 1)
        {
            Point2I center = new Point2I(_mapProvider.GetMap().GetHwid() / 2, _mapProvider.GetMap().GetHhgt() / 2);
            centers.Add(center);
        }
        if (centers.Count > 0)
        {
            _md.ZoneCenters.AddRange(centers);
        }
    }
}




using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RemoveSetupZonePatches : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        List<Point2I> deltas = new List<Point2I>();
        deltas.Add(new Point2I(-1, 0));
        deltas.Add(new Point2I(1, 0));
        deltas.Add(new Point2I(0, 1));
        deltas.Add(new Point2I(0, -1));

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed / 5);

        int numIterations = 0;
        bool somethingchanged = false;

        do
        {
            somethingchanged = false;
            numIterations++;
            List<Point3F> addedVals = new List<Point3F>();
            for (int x = 1; x < _mapProvider.GetMap().GetHwid() - 1; x++)
            {

                for (int z = 1; z < _mapProvider.GetMap().GetHhgt() - 1; z++)
                {
                    if (_md.MapZoneIds[x, z] <= MapConstants.MountainZoneId)
                    {
                        List<int> choices = new List<int>();
                        foreach (Point2I d in deltas)
                        {
                            short nearZoneId = _md.MapZoneIds[x + (int)(d.X), z + (int)(d.Z)];
                            if (nearZoneId > MapConstants.MountainZoneId)
                            {
                                choices.Add(nearZoneId);
                            }
                        }
                        if (choices.Count > 0)
                        {
                            int choice = choices[rand.Next() % choices.Count];
                            addedVals.Add(new Point3F(x, z, choice));
                            somethingchanged = true;
                        }
                    }
                }
            }

            foreach (Point3F val in addedVals)
            {
                _md.MapZoneIds[(int)(val.X), (int)(val.Z)] = (short)(val.Z);
            }
        }
        while (somethingchanged);
    }
}


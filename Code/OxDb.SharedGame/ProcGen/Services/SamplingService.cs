using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace OxDb.SharedGame.ProcGen.Services
{
    public interface ISamplingService : IInitializable
    {
        SamplingResult PlanePoissonSample(SamplingData sd);
    }
    public class SamplingService : ISamplingService
    {
        private INoiseService _noiseService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private List<Point2I> PlanePoissonSampleInternal(SamplingData sd)
        {
            List<Point2I> list = new List<Point2I>();
            if (sd == null)
            {
                return list;
            }

            if (sd.MinX >= sd.MaxX - 1 || sd.MinZ >= sd.MaxZ - 1)
            {
                return list;
            }

            if (sd.Count < 1 || sd.MaxAttemptsPerItem < 1)
            {
                return list;
            }

            MyRandom rand = null;
            if (sd.Seed > 0)
            {
                rand = new MyRandom(sd.Seed);
            }
            else
            {
                rand = new MyRandom();
            }

            float[,] noise = null;

            int width = sd.MaxX - sd.MinX + 1;
            int height = sd.MaxZ - sd.MinZ + 1;
            if (sd.NoiseAmp > 0 && sd.NoiseFreq > 0)
            {
                float pers = RandUtils.FloatRange(0.2f, 0.6f, rand);

                if (width <= 20000 && height <= 20000)
                {
                    noise = _noiseService.Generate(pers, sd.NoiseFreq, sd.NoiseAmp, 2, rand.Next(), width, height);
                }
            }

            int maxNumTimes = sd.Count * sd.MaxAttemptsPerItem;

            for (int i = 0; i < maxNumTimes && list.Count < sd.Count; i++)
            {
                Point2I newpt = new Point2I();
                newpt.X = RandUtils.IntRange(sd.MinX, sd.MaxX, rand);
                newpt.Z = RandUtils.IntRange(sd.MinZ, sd.MaxZ, rand);

                double newDist = GeomUtils.GetMinDistance2(list, newpt);

                double currSeparation = sd.MinSeparation;

                if (noise != null)
                {
                    int dx = MathUtil.Clamp(0, (int)(newpt.X - sd.MinX), width - 1);
                    int dy = MathUtil.Clamp(0, (int)(newpt.Z - sd.MinZ), height - 1);
                    currSeparation *= 1 + noise[dx, dy];
                    currSeparation = MathUtil.Clamp(sd.MinSeparation / 4, currSeparation, sd.MinSeparation * 2);
                }

                if (newDist > currSeparation)
                {
                    list.Add(newpt);
                }
            }

            return list;
        }

        public SamplingResult PlanePoissonSample(SamplingData sd)
        {
            List<Point2I> startPoints = PlanePoissonSampleInternal(sd);

            SamplingResult result = new SamplingResult()
            {
                MinX = sd.MinX,
                MaxX = sd.MaxX,
                MinZ = sd.MinZ,
                MaxZ = sd.MaxZ,

            };

            int index = 0;
            foreach (Point2I pt in startPoints)
            {
                result.Points.Add(new SampledPoint(pt.X, pt.Z, ++index));
            }

            if (sd.CreateIndexGrid)
            {
                CreateIndexGrid(result);
            }

            return result;
        }

        private void CreateIndexGrid(SamplingResult result)
        {

            if (result.Points.Count < 1)
            {
                return;
            }

            int width = result.MaxX - result.MinX + 1;
            int height = result.MaxZ - result.MinZ + 1;

            result.IndexGrid = new int[width, height];

            float xmid = (result.MinX + result.MaxX) / 2.0f;
            float zmid = (result.MinZ + result.MaxZ) / 2.0f;


            foreach (SampledPoint sp in result.Points)
            {
                float dx = sp.X - xmid;
                float dz = sp.Z - zmid;

                sp.DistanceFromCenter = Math.Sqrt(dx * dx + dz * dz);
            }

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    double minDistance = double.MaxValue;
                    int closestPointIndex = -1;

                    for (int i = 0; i < result.Points.Count; i++)
                    {
                        SampledPoint point = result.Points[i];

                        double deltaX = x - point.X + result.MinX;
                        double deltaZ = z - point.Z + result.MinZ;
                        double distanceSq = (deltaX * deltaX) + (deltaZ * deltaZ);

                        if (distanceSq < minDistance)
                        {
                            minDistance = distanceSq;
                            closestPointIndex = i;
                        }
                    }

                    result.IndexGrid[x, z] = closestPointIndex;
                }
            }

            return;
        }
    }
}



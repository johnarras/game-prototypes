using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace OxDb.SharedGame.ProcGen.Services
{
    public interface ISamplingService : IInitializable
    {
        List<MyPoint2> PlanePoissonSample(SamplingData sd);
        List<PointXZ> PlanePoissonSampleInteger(SamplingData sd);
    }
    public class SamplingService : ISamplingService
    {
        private INoiseService _noiseService = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public List<MyPoint2> PlanePoissonSample(SamplingData sd)
        {
            List<MyPoint2> list = new List<MyPoint2>();
            if (sd == null)
            {
                return list;
            }

            if (sd.XMin >= sd.XMax || sd.YMin >= sd.YMax)
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

            int width = (int)(sd.XMax - sd.XMin + 1);
            int height = (int)(sd.YMax - sd.YMin + 1);
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
                MyPoint2 newpt = new MyPoint2();
                newpt.X = RandUtils.FloatRange(sd.XMin, sd.XMax, rand);
                newpt.Y = RandUtils.FloatRange(sd.YMin, sd.YMax, rand);

                double newDist = GeomUtils.GetMinDistance2(list, newpt);

                double currSeparation = sd.MinSeparation;

                if (noise != null)
                {
                    int dx = MathUtil.Clamp(0, (int)(newpt.X - sd.XMin), width - 1);
                    int dy = MathUtil.Clamp(0, (int)(newpt.Y - sd.YMin), height - 1);
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

        public List<PointXZ> PlanePoissonSampleInteger(SamplingData sd)
        {
            List<MyPoint2> startPoints = PlanePoissonSample(sd);

            return startPoints.ConvertAll<PointXZ>(e => new PointXZ((int)Math.Round(e.X), (int)Math.Round(e.Y))).ToList();
        }
    }
}



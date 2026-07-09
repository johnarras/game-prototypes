using OxDb.SharedCore.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.ProcGen.Entities
{
    public class SamplingResult
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinZ { get; set; }
        public int MaxZ { get; set; }


        public List<SampledPoint> Points { get; set; } = new List<SampledPoint>();

        public int[,] IndexGrid { get; set; }

        public int GetIndexFromPos(int x, int z)
        {
            if (IndexGrid == null)
            {
                return 0;   
            }
            return IndexGrid[x - MinX, z - MinZ];
        }

    }

    public class SampledPoint : Point2I
    {
        public int Index { get; set; }
        public double DistanceFromCenter { get; set; }

        public SampledPoint(int x, int z, int index) : base(x,z)
        {
            Index = index; 
        }
    }
}

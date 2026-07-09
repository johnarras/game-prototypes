namespace OxDb.SharedGame.ProcGen.Entities
{
    public class SamplingData
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinZ { get; set; }
        public int MaxZ { get; set; }
        public int Count { get; set; }
        public double MinSeparation { get; set; }
        public int MaxAttemptsPerItem { get; set; }
        public long Seed { get; set; }
        public float NoiseAmp { get; set; } = 0.0f;
        public float NoiseFreq { get; set; } = 1.0f;
        public bool CreateIndexGrid { get; set; }
    }
}



using MessagePack;
namespace Genrpg.Shared.ProcGen.Entities
{
    public class SamplingData
    {
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public double ZMin { get; set; }
        public double ZMax { get; set; }
        public int Count { get; set; }
        public double MinSeparation { get; set; }
        public int MaxAttemptsPerItem { get; set; }
        public long Seed { get; set; }
        public float NoiseAmp { get; set; } = 0.0f;
        public float NoiseFreq { get; set; } = 1.0f;
    }
}



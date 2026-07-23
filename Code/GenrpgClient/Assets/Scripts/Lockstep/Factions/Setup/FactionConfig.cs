using OxDb.Client.Lockstep.Math;

namespace OxDb.Client.Lockstep.Factions.Setup
{
    public class FactionConfig
    {
        public int FactionId { get; set; }
        public uint SpawnInterval { get; set; }
        public int SpawnPercent { get; set; }

        public FixedPoint64 UnitSpeed { get; set; }
    }
}

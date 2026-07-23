using OxDb.Client.Lockstep.Math;
using Unity.Collections;

namespace OxDb.Client.Lockstep.Projectiles.Spawns
{
    [GenerateTestsForBurstCompatibility]
    public struct ProjectileSpawnData
    {
        public int Damage;
        public int FactionId;
        public int DurationTicks;
        public FixedPoint64 Speed;
    }
}

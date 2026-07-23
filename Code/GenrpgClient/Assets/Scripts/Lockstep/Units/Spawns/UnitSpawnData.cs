using OxDb.Client.Lockstep.Brains.Constants;
using OxDb.Client.Lockstep.Math;
using Unity.Collections;

namespace OxDb.Client.Lockstep.Units.Spawns
{
    [GenerateTestsForBurstCompatibility]
    public struct UnitSpawnData
    {
        public EBrainLogic BrainLogic;
        public int MaxHealth;
        public FixedPoint64 Speed;
    }
}

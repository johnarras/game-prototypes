using Assets.Scripts.Lockstep.Brains.Constants;
using Assets.Scripts.Lockstep.Math;
using Unity.Collections;

namespace Assets.Scripts.Lockstep.Units.Spawns
{
    [GenerateTestsForBurstCompatibility]
    public struct UnitSpawnData
    {
        public EBrainLogic BrainLogic;
        public int MaxHealth;
        public FixedPoint64 Speed;
    }
}

using Assets.Scripts.Lockstep.Math;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;

namespace Assets.Scripts.Lockstep.Projectiles.Spawns
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

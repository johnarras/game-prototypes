using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;

namespace Assets.Scripts.Lockstep.Buildings.Spawns
{
    [GenerateTestsForBurstCompatibility]
    public struct BuildingSpawnData
    {
        public int Tier; 
        [MarshalAs(UnmanagedType.U1)]
        public bool IsDefensive;
    }
}

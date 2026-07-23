using System.Runtime.InteropServices;
using Unity.Collections;

namespace OxDb.Client.Lockstep.Buildings.Spawns
{
    [GenerateTestsForBurstCompatibility]
    public struct BuildingSpawnData
    {
        public int Tier;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsDefensive;
    }
}

using System.Collections.Generic;

namespace OxDb.Client.Buildings
{
    public class BuildingMats : BaseBehaviour
    {
        public List<WeightedBuildingMaterial> WallMats = new List<WeightedBuildingMaterial>();
        public List<WeightedBuildingMaterial> DoorMats = new List<WeightedBuildingMaterial>();
        public List<WeightedBuildingMaterial> WindowMats = new List<WeightedBuildingMaterial>();
        public List<WeightedBuildingMaterial> ShinglesMats = new List<WeightedBuildingMaterial>();

        public List<WeightedBuildingMaterial> GetMatsFromSlot(EBuildingMatSlots slot)
        {
            if (slot == EBuildingMatSlots.Walls)
            {
                return WallMats;
            }
            else if (slot == EBuildingMatSlots.Doors)
            {
                return DoorMats;
            }
            else if (slot == EBuildingMatSlots.Windows)
            {
                return WindowMats;
            }
            else if (slot == EBuildingMatSlots.Shingles)
            {
                return ShinglesMats;
            }
            return WallMats;
        }

        public bool IsReady()
        {
            if (WallMats == null || WallMats.Count == 0 ||
                DoorMats == null || DoorMats.Count == 0 ||
                WindowMats == null || WindowMats.Count == 0 ||
                ShinglesMats == null || ShinglesMats.Count == 0)
            {
                return false;
            }

            return true;
        }

    }
}



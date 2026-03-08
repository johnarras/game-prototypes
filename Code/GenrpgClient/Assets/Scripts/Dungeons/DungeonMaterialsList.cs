using System.Collections.Generic;
namespace Assets.Scripts.Dungeons
{
    public class DungeonMaterialsList : BaseBehaviour
    {
        public List<WeightedDungeonMaterials> Materials = new List<WeightedDungeonMaterials>();

        public void Clear()
        {
            foreach (WeightedDungeonMaterials weighted in Materials)
            {
                weighted.Materials = null;
            }
        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }
    }
}



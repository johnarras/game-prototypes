using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Dungeons
{
    public class FinalDungeonMaterials
    {
        public FinalDungeonMaterials()
        {
            MatsBlock = new List<MaterialOption>[DungeonMaterialIndexes.Max];

            for (int i = 0; i < DungeonMaterialIndexes.Max; i++)
            {
                MatsBlock[i] = new List<MaterialOption>();
            }
        }

        // Must be in same order as the DungeonAssets asset lists.
        public List<MaterialOption>[] MatsBlock { get; set; } = new List<MaterialOption>[DungeonMaterialIndexes.Max];

        public List<MaterialOption> GetMaterials(int materialIndex)
        {

            if (materialIndex >= 0 && materialIndex < MatsBlock.Length)
            {
                return MatsBlock[materialIndex];
            }
            return null;
        }

        public void Clear()
        {
            for (int a = 0; a < DungeonMaterialIndexes.Max; a++)
            {
                List<MaterialOption> options = GetMaterials(a);

                foreach (MaterialOption opt in options)
                {
                    opt.Clear();
                }
            }
        }

        public bool IsReady()
        {
            if (MatsBlock == null || MatsBlock.Length < DungeonMaterialIndexes.Max)
            {
                return false;
            }

            for (int idx = 0; idx < DungeonMaterialIndexes.Max; idx++)
            {
                if (MatsBlock[idx] == null || MatsBlock[idx].Count < 1)
                {
                    return false;
                }

                for (int m = 0; m < MatsBlock[idx].Count; m++)
                {
                    if (!MatsBlock[idx][m].IsReady())
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}



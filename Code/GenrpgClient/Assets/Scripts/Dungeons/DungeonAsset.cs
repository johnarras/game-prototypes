using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Dungeons
{
    [Serializable]
    public class DungeonAsset : BaseBehaviour
    {

        public Animator Animator;

        public List<MeshRenderer> StoneRenderers = new List<MeshRenderer>();

        public List<MeshRenderer> WoodRenderers = new List<MeshRenderer>();

        public DungeonDoor Door { get; set; }

        public List<MeshRenderer> GetRenderersForMaterialIndex(int materialIndex)
        {
            if (materialIndex == DungeonMaterialIndexes.Stone)
            {
                return StoneRenderers;
            }
            else if (materialIndex == DungeonMaterialIndexes.Wood)
            {
                return WoodRenderers;
            }
            return StoneRenderers;
        }


        public async Awaitable SetOpened(bool openingNow, bool upperRightOfDoor)
        {

            if (Door != null)
            {
                List<Awaitable> allOpens = new List<Awaitable>();
                Door.PlayOpenSound(openingNow);
                foreach (DungeonDoorPanel door in Door.Panels)
                {
                    allOpens.Add(door.AnimateOpening(openingNow, upperRightOfDoor));
                }


                foreach (Awaitable aw in allOpens)
                {
                    await aw;
                }
            }
        }

        public void Clear()
        {
        }

        protected override void OnDestroy()
        {
            Clear();
        }
    }
}



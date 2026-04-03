using Assets.Scripts.Crawler.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class DungeonAsset : BaseBehaviour
    {
        private IAudioService _audioService = null;
        public Animator Animator;

        public List<MeshRenderer> StoneRenderers = new List<MeshRenderer>();

        public List<MeshRenderer> WoodRenderers = new List<MeshRenderer>();

        public List<MeshRenderer> FloorRenderers = new List<MeshRenderer>();

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
            else if (materialIndex == DungeonMaterialIndexes.Floors)
            {
                return FloorRenderers;
            }
            return StoneRenderers;
        }


        public bool SetOpened(bool isOpen)
        {
            if (WoodRenderers.Count > 0)
            {
                if (isOpen)
                {
                    _audioService.PlaySound(CrawlerAudio.DoorOpen, null);
                }
                foreach (MeshRenderer renderer in WoodRenderers)
                {
                    _clientEntityService.SetActive(renderer.gameObject, !isOpen);
                }
                return true;
            }
            return false;
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



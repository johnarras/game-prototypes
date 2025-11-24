using Assets.Scripts.Crawler.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class DungeonAsset : BaseBehaviour
    {
        private IAudioService _audioService;
        public Animator Animator;

        public List<MeshRenderer> Renderers = new List<MeshRenderer>();

        public List<MeshRenderer> DoorRenderers = new List<MeshRenderer>();


        public bool SetOpened(bool isOpen)
        {
            if (DoorRenderers.Count > 0)
            {
                if (isOpen)
                {
                    _audioService.PlaySound(CrawlerAudio.DoorOpen, null);
                }
                foreach (MeshRenderer renderer in DoorRenderers)
                {
                    _clientEntityService.SetActive(renderer.gameObject, !isOpen);
                }
                return true;
            }
            return false;
        }

        public void Clear()
        {
            Animator = null;
            Renderers.Clear();
            DoorRenderers.Clear();
        }

        protected override void OnDestroy()
        {
            Clear();
        }
    }
}

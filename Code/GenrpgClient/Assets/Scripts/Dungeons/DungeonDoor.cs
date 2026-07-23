using OxDb.Client.Audio.ClientEvents;
using OxDb.Client.Dungeons.Constants;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Dungeons
{
    [Serializable]
    public class DungeonDoor : BaseBehaviour, IWeightedItem
    {

        [field: SerializeField]
        public double Weight { get; set; }

        public EDoorOpenSounds OpenSound;

        public float WoodMaterialChance = 0.5f;
        public GameObject Anchor;

        public List<DungeonDoorPanel> Panels = new List<DungeonDoorPanel>();

        public void PlayOpenSound(bool openingNow)
        {
            if (openingNow && gameObject.activeInHierarchy)
            {
                _dispatcher.Dispatch(new PlaySound(OpenSound.ToString()));
            }
        }
    }
}

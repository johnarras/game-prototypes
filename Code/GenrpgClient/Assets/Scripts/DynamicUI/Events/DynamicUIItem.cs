using Assets.Scripts.WorldCanvas.Interfaces;
using UnityEngine;

namespace Assets.Scripts.WorldCanvas.GameEvents
{
    public enum DynamicUILocation
    {
        WorldSpace,
        ScreenSpace,
    };


    public class DynamicUIItem
    {
        public GameObject Go { get; set; }
        public IDynamicUIItem WCI { get; set; }
        public Vector3 StartPos { get; set; }
        public DynamicUILocation Location { get; set; }

        public DynamicUIItem(GameObject go, IDynamicUIItem wci, Vector3 startPos, DynamicUILocation location)
        {
            Go = go;
            WCI = wci;
            StartPos = startPos;
            Location = location;
        }
    }
}

using UnityEngine;

namespace Assets.Scripts.Doobers.Events
{
    public class ShowDooberEvent
    {
        public Vector3 StartPosition;
        public long EntityTypeId;
        public long EntityId;
        public long Quantity;
        public string AtlasName;
        public string SpriteName;
        public Vector3 EndPosition;
        public bool PointAtEnd;
        public float StartOffsetSize;
        public bool Accelerate;
        public bool StartsInUI;
        public float LerpTime = 0.5f;
        public double SizeScale = 1.0f;

    }
}

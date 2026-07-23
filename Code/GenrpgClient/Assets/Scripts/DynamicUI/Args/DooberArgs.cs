using UnityEngine;

namespace OxDb.Client.Doobers.Events
{
    public class DooberArgs
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
        public float PercentDonePowerMult = 0;
        public bool StartsInUI;
        public float LerpTime = 1.0f;
        public double SizeScale = 1.0f;



        public void Clear()
        {
            StartPosition = Vector3.zero;
            EntityTypeId = 0;
            EntityId = 0;
            Quantity = 0;
            AtlasName = null;
            SpriteName = null;
            EndPosition = Vector3.zero;
            StartOffsetSize = 0;
            PercentDonePowerMult = 0;
            StartsInUI = false;
            LerpTime = 1.0f;
            SizeScale = 1.0f;
        }

    }
}



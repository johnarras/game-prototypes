using System;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class MaterialOption
    {
        public Material Mat;

        public void Clear()
        {
            Mat = null;


        }

        public bool IsReady()
        {
            return Mat != null && Mat.mainTexture != null;
        }
    }
}



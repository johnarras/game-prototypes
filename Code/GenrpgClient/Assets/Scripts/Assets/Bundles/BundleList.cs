using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OxDb.Client.Assets.Bundles
{
    public class BundleList : ScriptableObject
    {
        public List<BundleInfo> Bundles = new List<BundleInfo>();


#if UNITY_EDITOR
        [MenuItem("Assets/Create/ScriptableObjects/BundleList", false, 0)]
        public static void Create()
        {
            ScriptableObjectUtils.CreateBasicInstance<BundleList>();
        }
#endif

    }

    [Serializable]
    public class BundleInfo
    {
        public string BundleName;
        public bool IsLocal;
    }
}



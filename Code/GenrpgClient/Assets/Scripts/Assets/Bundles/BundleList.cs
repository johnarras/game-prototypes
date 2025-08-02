using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Assets.Bundles
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

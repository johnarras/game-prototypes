using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Assets.Entities
{
    public class BundleDownload
    {
        public string url;
        public string bundleName;
        public string assetName;
        public OnDownloadHandler handler;
        public System.Object data;
        public GameObject parent;
        public bool isLocal;
        public int idHash;
        public CancellationToken Token;
    }

}

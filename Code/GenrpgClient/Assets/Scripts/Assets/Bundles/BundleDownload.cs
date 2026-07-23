using OxDb.SharedCore.Logalytics.Interfaces;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Assets.Entities
{
    public interface IBundleDownload
    {
        string url { get; set; }
        string bundleName { get; set; }
        string assetName { get; set; }
        GameObject parent { get; set; }
        bool isLocal { get; set; }
        int idHash { get; set; }
        CancellationToken Token { get; set; }

        void CallDownloadHandler(GameObject asset, ILogService logService);
    }


    public class BundleDownload<T> : IBundleDownload
    {
        public string url { get; set; }
        public string bundleName { get; set; }
        public string assetName { get; set; }
        public AssetDownloadHandler<T> handler { get; set; }
        public T data { get; set; }
        public GameObject parent { get; set; }
        public bool isLocal { get; set; }
        public int idHash { get; set; }
        public CancellationToken Token { get; set; }

        public void CallDownloadHandler(GameObject asset, ILogService logService)
        {
            if (asset == null)
            {
                logService.Info("Failed To load asset: " + assetName + " from " + bundleName);
            }
            else if (handler != null)
            {
                handler(asset, data, Token);
            }

        }
    }

}



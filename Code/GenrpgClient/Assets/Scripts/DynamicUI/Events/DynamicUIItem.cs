using Assets.Scripts.Assets.Entities;
using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Client.Assets.Constants;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.WorldCanvas.GameEvents
{
    public enum DynamicUILocation
    {
        WorldSpace,
        ScreenSpace,
    };

    public class ShowDynamicUIItem
    {
        public string AssetCategory { get; set; } = AssetCategoryNames.UI;
        public string AssetName { get; set; }
        public string Subdirectory { get; set; }
        public Vector3 StartPos { get; set; }
        public DynamicUILocation Location { get; set; }
        public object Data { get; set; }
        public OnDownloadHandler Handler { get; set; }
        public CancellationToken Token { get; set; }

        public ShowDynamicUIItem(DynamicUILocation location, string assetName, Vector3 startPos,
            OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null)
        {
            Location = location;
            AssetName = assetName;
            StartPos = startPos;
            Handler = handler;
            Data = data;
            Token = token;
            Subdirectory = subdirectory;
        }
    }


    public class DynamicUIItem
    {
        public Object Data { get; set; }
        public GameObject Go { get; set; }
        public IDynamicUIItem WCI { get; set; }
        public Vector3 StartPos { get; set; }
        public DynamicUILocation Location { get; set; }
        public ObjectPool Pool { get; set; }

        public DynamicUIItem(GameObject go, IDynamicUIItem wci, Vector3 startPos, DynamicUILocation location, ObjectPool pool)
        {
            Go = go;
            WCI = wci;
            StartPos = startPos;
            Location = location;
            Pool = pool;
        }
    }
}

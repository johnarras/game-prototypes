using Assets.Scripts.Assets.Entities;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.WorldCanvas.GameEvents
{
    public enum DynamicUILocation
    {
        WorldSpace,
        ScreenSpace,
    };

    public class ShowDynamicUIItem : IClientEvent
    {
        public string AssetCategory { get; set; } = AssetCategoryNames.UI;
        public string AssetName { get; set; }
        public string Subdirectory { get; set; }
        public Vector3 StartPos { get; set; }
        public DynamicUILocation Location { get; set; }
        public object Data { get; set; }
        public AssetDownloadHandler<object> Handler { get; set; }
        public CancellationToken Token { get; set; }

        public ShowDynamicUIItem(DynamicUILocation location, string assetName, Vector3 startPos,
            AssetDownloadHandler<object> handler, object data, CancellationToken token, string subdirectory = null)
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


    public class DynamicUIItem : IClientEvent
    {
        public Object Data { get; set; }
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



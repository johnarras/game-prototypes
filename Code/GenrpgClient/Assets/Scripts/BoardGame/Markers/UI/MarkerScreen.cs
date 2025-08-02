using Genrpg.Shared.BoardGame.Markers.WebApi;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Users.PlayerData;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Markers.UI
{
    public class MarkerScreen : BaseScreen
    {
        public GameObject ContentRoot;

        public string MarkerIconPrefabName = "MarkerIcon";
        private List<MarkerIcon> _markerIcons = new List<MarkerIcon>();

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            InitMarkerIcons();
            _dispatcher.AddListener<SetMarkerResponse>(OnSetMarkerResponse, GetToken());
            await Task.CompletedTask;
        }

        private void InitMarkerIcons()
        {
            _awaitableService.ForgetAwaitable(InitMarkerIconsAsync());
        }


        private bool _initializing = false;
        private async Awaitable InitMarkerIconsAsync()
        {
            if (_initializing)
            {
                return;
            }

            CoreUserData coreUserData = _gs.ch.Get<CoreUserData>(); 

            _initializing = true;
            IReadOnlyList<Marker> allMarkers = _gameData.Get<MarkerSettings>(_gs.user).GetData();

            foreach (Marker marker in allMarkers)
            {
                MarkerIcon currIcon = _markerIcons.FirstOrDefault(x => x.MarkerId() == marker.IdKey);

                if (currIcon == null)
                {
                    _assetService.LoadAssetInto(ContentRoot, AssetCategoryNames.UI, MarkerIconPrefabName, OnLoadMarker,
                        marker, GetToken(), Subdirectory);
                }
                else
                {
                    currIcon.SetData(this,marker, coreUserData);
                }
            }


            _initializing = false;
            await Task.CompletedTask;
        }

        private void OnSetMarkerResponse(SetMarkerResponse response)
        {
            if (!response.Success)
            {
                return;
            }
            InitMarkerIcons();
        }

        private void OnLoadMarker(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            Marker marker = data as Marker;

            if (marker == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            MarkerIcon icon = go.GetComponent<MarkerIcon>();
            if (icon == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            _markerIcons.Add(icon);
            icon.SetData(this, marker, _gs.ch.Get<CoreUserData>());
        }
    }
}

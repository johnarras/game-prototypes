using Assets.Scripts.Assets;
using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Players;
using Assets.Scripts.BoardGame.Tiles;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.BoardGame.Markers.WebApi;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.BoardGame.Markers.Services
{
    public interface IClientMarkerService : IInjectable
    {
        void ServerSetMarkerId(long markerId, long markerTier, CancellationToken token);
        void ClientSetMarkerId(long markerId, long markerTier);
    }
    public class ClientMarkerService : IClientMarkerService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IBoardGameController _boardGameController = null;
        private IAssetService _assetService = null;
        private IClientEntityService _clientEntityService = null;
        private IPlayerManager _playerManager = null;
        private IClientWebService _webService = null;
        private IScreenService _screenService = null;
            

        public void ServerSetMarkerId(long markerId, long markerTier, CancellationToken token)
        {
            _webService.SendClientUserWebRequest(new SetMarkerRequest() { MarkerId = markerId, MarkerTier = markerTier }, token);
        }
        public void ClientSetMarkerId(long markerId, long markerTier)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            BoardData boardData = _gs.ch.Get<BoardData>();  

            Marker marker = _gameData.Get<MarkerSettings>(_gs.ch).Get(markerId);

            if (marker == null || marker.MaxTier < markerTier)
            {
                marker = _gameData.Get<MarkerSettings>(_gs.ch).Get(1);
                markerId = 1;
                markerTier = 1;
            }
            userData.Vars.Set(UserVars.MarkerId, markerId);
            userData.Vars.Set(UserVars.MarkerTier, markerTier); 

            ClientTile tile = _boardGameController.GetTile(boardData.TileIndex);

            if (tile != null)
            {
                _assetService.LoadAssetInto(tile.PieceAnchor, AssetCategoryNames.Markers, marker.Art + markerTier, OnLoadMarker, tile.PieceAnchor, _boardGameController.GetToken());
            }
        }
        private void OnLoadMarker(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            GameObject anchor = data as GameObject;

            if (anchor == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            GameObject playerObject = _playerManager.GetPlayerGameObject();
            if (playerObject == null)
            {
                playerObject = new GameObject();
                playerObject.AddComponent<PlayerMarker>();
                _clientEntityService.AddToParent(playerObject, anchor);
                _playerManager.SetPlayerObject(playerObject);
            }

            _clientEntityService.DestroyAllChildren(playerObject);
            PlayerMarker playerMarker = playerObject.GetComponent<PlayerMarker>();  
            playerMarker.View = go;
            _clientEntityService.AddToParent(go, playerObject);
            _screenService.Close(ScreenNames.Marker);

        }
    }
}

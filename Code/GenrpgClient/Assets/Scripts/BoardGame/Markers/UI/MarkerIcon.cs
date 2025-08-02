using Assets.Scripts.BoardGame.Markers.Services;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;

namespace Assets.Scripts.BoardGame.Markers.UI
{
    public class MarkerIcon : BaseBehaviour
    {

        private IClientMarkerService _clientMarkerService = null;

        public GText Name;
        public GImage Icon;
        public GButton Button;
        public GImage ActiveHighlight;

        private Marker _marker;
        public void SetData(MarkerScreen screen, Marker marker, CoreUserData userData)
        {
            _marker = marker;
            _assetService.LoadEntityIcon(EntityTypes.Marker, marker.IdKey, Icon, GetToken());
            _uiService.SetText(Name, marker.Name);
            _uiService.SetButton(Button, screen.GetName(), OnClickButton);

            _clientEntityService.SetActive(ActiveHighlight, _marker.IdKey == userData.Vars.Get(UserVars.MarkerId));
        }

        public long MarkerId()
        {
            return _marker?.IdKey ?? -1;
        }

        private void OnClickButton()
        {
            _clientMarkerService.ServerSetMarkerId(_marker.IdKey, 1, GetToken());
        }
    }
}

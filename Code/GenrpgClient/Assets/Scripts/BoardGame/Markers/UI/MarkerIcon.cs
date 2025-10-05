using Assets.Scripts.BoardGame.Markers.Services;
using Assets.Scripts.Entities.UI;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;

namespace Assets.Scripts.BoardGame.Markers.UI
{
    public class MarkerIcon : EntityIcon
    {

        private IClientMarkerService _clientMarkerService = null;

        public GButton Button;
        public GImage ActiveHighlight;

        private Marker _marker;
        public void SetMarkerData(MarkerScreen screen, Marker marker, CoreUserData userData)
        {
            SetEntityData(EntityTypes.Marker, marker.IdKey, 1, 1);
            _marker = marker;
            _uiService.SetText(NameText, marker.Name);
            _uiService.SetButton(Button, screen.GetName(), OnClickButton);

            _clientEntityService.SetActive(ActiveHighlight, _marker.IdKey == userData.Vars.Get(UserVars.MarkerId));
        }

        private void OnClickButton()
        {
            _clientMarkerService.ServerSetMarkerId(_marker.IdKey, 1, GetToken());
        }
    }
}

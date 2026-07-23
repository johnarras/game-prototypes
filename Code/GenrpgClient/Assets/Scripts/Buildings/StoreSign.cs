using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.MapObjects.Messages;

namespace OxDb.Client.Buildings
{
    public class StoreSign : BaseBehaviour
    {
        public GText SignText;
        public GImage BGImage;

        private BuildingType _btype;
        private OnSpawn _spawn;
        public void Init(BuildingType btype, OnSpawn spawn, string overrideName = null)
        {
            _btype = btype;
            _spawn = spawn;

            if (!btype.ShowNameplate)
            {
                _clientEntityService.SetActive(gameObject, false);
                return;
            }

            _uiService.SetText(SignText, !string.IsNullOrEmpty(overrideName) ? overrideName : _btype.Name);
        }
    }
}



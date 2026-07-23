using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.MapObjects.Messages;

namespace OxDb.Client.Buildings
{
    public class MapBuilding : BaseBehaviour
    {
        public StoreSign Sign;

        private BuildingType _btype;
        private OnSpawn _spawn;
        public void Init(BuildingType btype, OnSpawn spawn, string overrideName = null)
        {
            _btype = btype;
            _spawn = spawn;
            name = btype.Name + "Building";
            Sign?.Init(_btype, _spawn, overrideName);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}



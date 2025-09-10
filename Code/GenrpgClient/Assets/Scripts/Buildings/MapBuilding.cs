using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.MapObjects.Messages;

namespace Assets.Scripts.Buildings
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
    }
}

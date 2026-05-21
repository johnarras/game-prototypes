using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
namespace Assets.Scripts.GameSettings.Entities
{
    public class ClientGameData : GameData
    {
        private IFilteredObject _obj = null;
        public override T Get<T>(IFilteredObject obj)
        {
            if (obj == null)
            {
                obj = _obj;
            }
            return base.Get<T>(obj);
        }

        public void SetSettingsObject(IFilteredObject obj)
        {
            _obj = null;
        }
    }
}



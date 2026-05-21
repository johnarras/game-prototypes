using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Buildings
{
    public abstract class BuildingStateHelper : BaseStateHelper
    {
        protected virtual int GetBuildingImageIndex(PartyData party, long buildingTypeId)
        {
            BuildingType btype = _gameData.Get<BuildingSettings>(_gs.ch).Get(buildingTypeId);

            return 1;
        }
    }
}



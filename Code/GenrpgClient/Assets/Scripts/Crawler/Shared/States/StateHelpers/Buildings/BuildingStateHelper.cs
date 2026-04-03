using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Buildings
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



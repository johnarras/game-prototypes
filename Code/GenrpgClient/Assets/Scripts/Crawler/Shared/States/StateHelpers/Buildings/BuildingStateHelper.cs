using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Buildings
{
    public abstract class BuildingStateHelper : BaseStateHelper
    {
        protected virtual int GetBuildingImageIndex(PartyData party, long buildingTypeId)
        {
            BuildingType btype = _gameData.Get<BuildingSettings>(_gs.ch).Get(buildingTypeId);

            if (btype == null || btype.VariationCount <= 1)
            {
                return 1;
            }

            int index = (party.CurrPos.X * 11 + party.CurrPos.Z * 31) % btype.VariationCount + 1;
            return index;
        }
    }
}

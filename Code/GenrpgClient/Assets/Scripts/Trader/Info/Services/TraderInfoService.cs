using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Roads.Settings;
using System.Text;

namespace Assets.Scripts.Trader.Info.Services
{
    public interface ITraderInfoService : IInjectable
    {
        string GetHUDStatus(CoreData coreData);
    }

    public class TraderInfoService : ITraderInfoService
    {
        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;

        public string GetHUDStatus(CoreData coreData)
        {
            CaravanPosition pos = _caravanService.GetPosition(coreData);

            Road onRoad = _gameData.Get<RoadSettings>(coreData).Get(pos.RoadId);

            City inCity = _gameData.Get<CitySettings>(coreData).Get(pos.CityId);

            City targetCity = _gameData.Get<CitySettings>(coreData).Get(pos.TargetCityId);

            City outsideCity = _gameData.Get<CitySettings>(coreData).Get(pos.OutsideOfCityId);

            StringBuilder sb = new StringBuilder();

            if (outsideCity != null)
            {
                sb.Append("You are outside " + outsideCity.Name);

                if (onRoad != null)
                {
                    sb.Append(" You are on " + onRoad.Name);

                    if (targetCity != null)
                    {
                        sb.Append(" heading toward " + targetCity.Name);
                    }
                }
            }
            else if (onRoad != null)
            {
                sb.Append("You are on " + onRoad.Name + " " + coreData.Vars[TraderVars.DistanceAlongRoad] +
                    "/" + onRoad.Distance + " " + TraderConstants.DistanceAbbreviation);

                if (targetCity != null)
                {
                    sb.Append(" toward " + targetCity.Name);
                }
            }
            else if (inCity != null)
            {
                sb.Append("You are in " + inCity.Name);
            }
            else
            {
                sb.Append("You are somewhere strange...");
            }

            return sb.ToString();
        }
    }
}

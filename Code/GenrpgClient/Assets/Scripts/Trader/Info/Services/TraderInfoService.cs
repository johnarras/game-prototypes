using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using System.Text;

namespace Assets.Scripts.Trader.Info.Services
{
    public interface ITraderInfoService : IInjectable
    {
        string GetHUDStatus(CoreData coreData);
    }

    public class TraderInfoService : ITraderInfoService
    {
        private ICaravanService _caravanService = null;

        public string GetHUDStatus(CoreData coreData)
        {
            CaravanPosition pos = _caravanService.GetPosition(coreData);

            StringBuilder sb = new StringBuilder();

            if (pos.GetCurrentCity() != null)
            {
                sb.Append("You are in " + pos.GetCurrentCity().Name);
            }
            else
            {
                if (pos.TargetCity != null)
                {
                    sb.Append("Heading toward " + pos.TargetCity.Name);
                }
                else
                {
                    sb.Append("Wilderness: ");
                }
                sb.Append(" (" + pos.DistanceGone + "/" + pos.TotalDistanceToTarget + " " + TraderConstants.DistanceAbbreviation + ")");
            }

            return sb.ToString();
        }
    }
}

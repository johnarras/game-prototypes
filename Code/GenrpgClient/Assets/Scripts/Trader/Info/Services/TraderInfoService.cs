using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.Trader.Constants;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Info.Services
{
    public interface ITraderInfoService : IInjectable
    {
        ValueTask<string> GetHUDStatus();
    }

    public class TraderInfoService : ITraderInfoService
    {
        private ICaravanService _caravanService = null;
        private IClientGameState _gs = null;

        public async ValueTask<string> GetHUDStatus()
        {
            CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

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

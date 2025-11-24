using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.UserEnergy.WebApi;

namespace Genrpg.RequestServer.Resets.Services
{
    public class PeriodicUpdateService : IPeriodicUpdateService
    {
        private IGameData _gameData = null;
        public async Task CheckHourlyCurrencyUpdate(WebContext context)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            IReadOnlyList<CoreCurrencyType> currencies = _gameData.Get<CoreCurrencyTypeSettings>(context.user).GetData();

            List<CoreCurrencyStatus> changedStatuses = new List<CoreCurrencyStatus>();


            DateTime currTime = DateTime.UtcNow;
            foreach (CoreCurrencyType ctype in currencies)
            {
                CoreCurrencyStatus status = userData.Currencies.GetStatus(ctype.IdKey);

                if (status.Curr() >= status.Storage())
                {
                    continue;
                }

                if (status.Regen() < 1)
                {
                    continue;
                }

                if (status.NextRegenTick > currTime)
                {
                    continue;
                }

                double totalHoursFract = (currTime - status.NextRegenTick).TotalHours;

                int wholeTotalHours = (int)totalHoursFract;

                if (wholeTotalHours < totalHoursFract)
                {
                    wholeTotalHours++;
                }

                long totalRegenPossible = status.Storage() - status.Curr();

                long totalRegenNow = Math.Min(totalRegenPossible, wholeTotalHours * status.Regen());

                status.AddCurr(totalRegenNow);

                status.NextRegenTick.AddHours(wholeTotalHours);

                if (status.Curr() >= status.Storage())
                {
                    status.NextRegenTick = DateTime.UtcNow;
                }

                if (totalRegenNow > 0)
                {
                    changedStatuses.Add(status);
                }
            }

            context.Responses.AddResponse(new UpdateCoreCurrenciesResponse() { ChangedStatuses = changedStatuses });
        }
    }
}

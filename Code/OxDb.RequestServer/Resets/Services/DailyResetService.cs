using OxDb.RequestServer.Core;
using OxDb.RequestServer.Resets.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Resets.PlayerData;
using OxDb.SharedGame.Resets.Settings;
using OxDb.SharedGame.Time.Services;

namespace OxDb.RequestServer.Resets.Services
{
    public interface IDailyResetService : IInjectable
    {
        ValueTask DailyReset(WebContext context);
    }
    public class DailyResetService : IDailyResetService
    {


        protected IGameData _gameData;
        protected ITimeService _timeService = null;

        private OrderedSetupDictionaryContainer<EDailyResetOrder, IDailyResetHelper> _resetHelpers = new OrderedSetupDictionaryContainer<EDailyResetOrder, IDailyResetHelper>();
        //private List<IResetHelper> _helpers = null;

        public async ValueTask DailyReset(WebContext context)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            ResetSettings settings = _gameData.Get<ResetSettings>(coreData);

            ResetData resetData = await context.GetAsync<ResetData>();


            DateTime currTime = _timeService.GetTime(coreData);

            DateTime lastResetDate = resetData.LastResetDay.Date;
            DateTime nextResetTime = resetData.LastResetDay.Date.AddDays(1).AddHours(settings.ResetHour);


            DateTime todayDate = currTime.Date;

            DateTime todayResetTime = todayDate.AddHours(settings.ResetHour);

            int dayDiff = (todayDate - lastResetDate).Days;

            DateTime currentResetDay = currTime.Date;
            // Before reset, needed to have reset before yesterday.
            if (currTime < todayResetTime)
            {
                if (dayDiff <= 1)
                {
                    return;
                }
                else
                {
                    dayDiff--;
                }
                currentResetDay = currentResetDay.AddDays(-1);
            }

            if (dayDiff < 1)
            {
                return;
            }

            int daysMissed = dayDiff - 1;

            if (daysMissed > 0)
            {
                resetData.ConsecutiveResetDays = 0;
            }
            else
            {
                resetData.ConsecutiveResetDays++;
            }

            resetData.LastResetDay = currentResetDay;


            DateTime lastAcceptableResetTime = todayResetTime.Date.AddDays(-1);

            foreach (IDailyResetHelper helper in _resetHelpers.OrderedItems())
            {
                await helper.DailyReset(context, resetData.ConsecutiveResetDays, daysMissed);
            }

            await Task.CompletedTask;
        }
    }
}



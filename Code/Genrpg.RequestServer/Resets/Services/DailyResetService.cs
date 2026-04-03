using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Interfaces;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Resets.PlayerData;
using Genrpg.Shared.Resets.Settings;
using Genrpg.Shared.Time.Services;

namespace Genrpg.RequestServer.Resets.Services
{
    public class DailyResetService : IDailyResetService
    {


        protected IGameData _gameData;
        protected ITimeService _timeService = null;

        private OrderedSetupDictionaryContainer<Type, IDailyResetHelper> _resetHelpers = new OrderedSetupDictionaryContainer<Type, IDailyResetHelper>();
        //private List<IResetHelper> _helpers = null;

        public async Task DailyReset(WebContext context)
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



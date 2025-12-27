using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Time.Settings;
using System;

namespace Genrpg.Shared.Time.Services
{
    public interface ITimeService : IInjectable
    {
        DateTime GetTime(IFilteredObject obj);
    }

    public class TimeService : ITimeService
    {

        private IGameData _gameData = null;

        public DateTime GetTime(IFilteredObject obj)
        {
            TimeSettings settings = _gameData.Get<TimeSettings>(obj);

            if (settings.UseOverrideTime)
            {
                return settings.OverrideTime;
            }
            return DateTime.UtcNow;
        }
    }
}



using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Time.Settings;
using System;

namespace OxDb.SharedGame.Time.Services
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



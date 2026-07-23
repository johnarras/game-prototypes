using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.Resets.Interfaces
{

    public enum EDailyResetOrder
    {

    }

    public interface IDailyResetHelper : IOrderedSetupDictionaryItem<EDailyResetOrder>
    {
        Task DailyReset(WebContext context, int consecutiveResetDays, int daysMissed);
    }
}



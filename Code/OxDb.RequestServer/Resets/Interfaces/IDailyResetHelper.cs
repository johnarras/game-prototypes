using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.Resets.Interfaces
{
    public interface IDailyResetHelper : IOrderedSetupDictionaryItem<Type>
    {
        Task DailyReset(WebContext context, int consecutiveResetDays, int daysMissed);
    }
}



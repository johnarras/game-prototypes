using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Resets.Interfaces
{
    public interface IDailyResetHelper : IOrderedSetupDictionaryItem<Type>
    {
        Task DailyReset(WebContext context, int consecutiveResetDays, int daysMissed);
    }
}



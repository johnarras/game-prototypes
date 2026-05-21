using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.Resets.Services
{
    public interface IDailyResetService : IInjectable
    {
        Task DailyReset(WebContext context);
    }
}



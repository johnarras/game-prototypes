using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Resets.Services
{
    public interface IDailyResetService : IInjectable
    {
        Task DailyReset(WebContext context);
    }
}



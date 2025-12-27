using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Resets.Services
{
    public interface IHourlyUpdateService : IInjectable
    {
        Task CheckHourlyCurrencyUpdate(WebContext context, bool onLogin);
    }
}



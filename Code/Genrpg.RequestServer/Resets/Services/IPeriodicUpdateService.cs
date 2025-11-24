using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Resets.Services
{
    public interface IPeriodicUpdateService : IInjectable
    {
        Task CheckHourlyCurrencyUpdate(WebContext context);
    }
}

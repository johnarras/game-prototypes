using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Resets.Services
{
    public interface IHourlyUpdateService : IInjectable
    {
        Task CheckHourlyCurrencyUpdates(WebContext context, HourlyResetArgs args);
    }
}



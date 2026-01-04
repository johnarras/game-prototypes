using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Entities;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Resets.Services
{
    public interface IHourlyUpdateService : IInjectable
    {
        Task CheckHourlyCurrencyUpdate(WebContext context, HourlyResetArgs args);
    }
}



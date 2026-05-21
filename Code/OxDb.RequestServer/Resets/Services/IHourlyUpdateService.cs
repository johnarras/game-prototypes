using OxDb.RequestServer.Core;
using OxDb.RequestServer.Resets.Entities;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.Resets.Services
{
    public interface IHourlyUpdateService : IInjectable
    {
        Task CheckHourlyCurrencyUpdates(WebContext context, HourlyResetArgs args);
    }
}



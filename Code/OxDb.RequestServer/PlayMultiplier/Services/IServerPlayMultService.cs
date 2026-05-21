using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;

namespace OxDb.RequestServer.PlayMultiplier.Services
{
    public interface IServerPlayMultService : IInjectable
    {
        Task SetPlayMult(WebContext context, int newPlayMult);
    }
}



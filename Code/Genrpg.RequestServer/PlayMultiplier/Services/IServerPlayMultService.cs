using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.PlayMultiplier.Services
{
    public interface IServerPlayMultService : IInjectable
    {
        Task SetPlayMult(WebContext context, int newPlayMult);
    }
}



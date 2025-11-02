using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Core.Interfaces
{
    public interface IClientResetCleanup : IInjectable
    {
        Task OnClientResetCleanup(CancellationToken token);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Core.Interfaces
{
    public interface IClientResetCleanup
    {
        Task OnReset(CancellationToken token);
    }
}



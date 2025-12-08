using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Core.Interfaces
{
    public interface IClientResetCleanup
    {
        Task OnReset(CancellationToken token);
    }
}

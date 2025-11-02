using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Genrpg.Shared.Client.Tokens
{
    public interface IGameTokenService : IInjectable
    {
        void SetGameToken(CancellationToken token);
    }
}

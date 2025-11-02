using Genrpg.Shared.Interfaces;
using System.Threading;

namespace Genrpg.Shared.Client.Tokens
{
    public interface IMapTokenService : IInjectable
    {
        void SetMapToken(CancellationToken token);
    }
}

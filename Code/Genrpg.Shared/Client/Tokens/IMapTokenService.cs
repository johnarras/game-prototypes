using System.Threading;

namespace Genrpg.Shared.Client.Tokens
{
    public interface IMapTokenService
    {
        void SetMapToken(CancellationToken token);
    }
}



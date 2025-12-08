using System.Threading;

namespace Genrpg.Shared.Client.Tokens
{
    public interface IGameTokenService
    {
        void SetGameToken(CancellationToken token);
    }
}

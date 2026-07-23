using System.Threading;

namespace OxDb.Client.Setup.Interfaces
{
    public interface IGameTokenService
    {
        void SetGameToken(CancellationToken token);
    }
}

using System.Threading;

namespace OxDb.Client.Setup.Interfaces
{
    public interface IMapTokenService
    {
        void SetMapToken(CancellationToken token);
    }
}

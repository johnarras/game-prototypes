using System.Threading;

namespace Assets.Scripts.Setup.Interfaces
{
    public interface IMapTokenService
    {
        void SetMapToken(CancellationToken token);
    }
}

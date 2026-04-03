using System.Threading;

namespace Assets.Scripts.Setup.Interfaces
{
    public interface IGameTokenService
    {
        void SetGameToken(CancellationToken token);
    }
}

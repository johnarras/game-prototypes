using System.Threading;

namespace OxDb.SharedGame.UI.Interfaces
{
    public interface IButton
    {
        CancellationToken GetToken();
    }
}



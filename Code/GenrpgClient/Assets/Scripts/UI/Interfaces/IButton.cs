using System.Threading;

namespace Genrpg.Shared.UI.Interfaces
{
    public interface IButton
    {
        CancellationToken GetToken();
    }
}



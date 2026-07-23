using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.UI.Interfaces
{
    public interface IScreen
    {
        long ScreenId { get; }
        Task StartOpen(object data, CancellationToken token);
        void StartClose();
        void ErrorClose(string txt);
        void OnInfoChanged();
        bool BlockMouse();
        string GetName();
        CancellationToken GetToken();
    }
}



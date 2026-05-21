using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Shipments.UI
{
    public class ManifestScreen : BaseScreen
    {
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

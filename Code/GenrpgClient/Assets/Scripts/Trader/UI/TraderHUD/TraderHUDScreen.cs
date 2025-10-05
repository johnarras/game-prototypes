using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.TraderHUD
{
    public class TraderHUDScreen : BaseScreen
    {
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

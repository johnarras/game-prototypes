using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Estates.UI
{
    public class EstateScreen : BaseScreen
    {
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

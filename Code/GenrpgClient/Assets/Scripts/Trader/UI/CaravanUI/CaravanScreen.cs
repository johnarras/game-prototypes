using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.CaravanUI
{
    public class CaravanScreen : BaseScreen
    {
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

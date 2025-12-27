
using Assets.Scripts.UI.ScreenSystem;
using System.Threading;
using System.Threading.Tasks;

public class BlockerScreen : BaseScreen
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await Task.CompletedTask;
    }
}




using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.UI.ScreenSystem;
using System.Threading;
using System.Threading.Tasks;


public class LoadingScreen : BaseScreen
{
    protected IAudioService _audioService = null;
    protected IObjectPool _objectPool = null;
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        // Play music null plays music track 1 and ambient track 0 (none)
        _audioService.PlayMusic(null);

        await Task.CompletedTask;
    }
}




using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.ScreenSystem
{
    public abstract class TypedArgScreen<T> : BaseScreen where T : class
    {
        protected abstract Task OnStartOpen(T obj, CancellationToken cancellationToken);

        protected override async Task OnStartOpen(object obj, CancellationToken token)
        {
            await OnStartOpen(obj as T, token);
        }
    }
}

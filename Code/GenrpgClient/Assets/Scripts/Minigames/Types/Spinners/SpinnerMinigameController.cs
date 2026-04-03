using Assets.Scripts.Minigames.Controllers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Minigames.Types.Spinners
{
    public class SpinnerMinigameController : BaseMinigameController
    {
        protected override async Awaitable OnSetDataAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

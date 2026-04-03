using Assets.Scripts.Minigames.Controllers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Minigames.Types.Clickers
{
    public class ClickerMinigameController : BaseMinigameController
    {
        protected override async Awaitable OnSetDataAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

using OxDb.Client.Minigames.Controllers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Minigames.Types.Spinners
{
    public class SpinnerMinigameController : BaseMinigameController
    {
        protected override async Awaitable OnSetDataAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

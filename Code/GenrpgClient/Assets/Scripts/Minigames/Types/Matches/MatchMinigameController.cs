using Assets.Scripts.Minigames.Controllers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Minigames.Types.Matches
{
    public class MatchMinigameController : BaseMinigameController
    {
        protected override async Awaitable OnSetDataAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}

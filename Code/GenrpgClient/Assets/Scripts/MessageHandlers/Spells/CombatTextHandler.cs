
using OxDb.SharedGame.Spells.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class CombatTextHandler : BaseClientMapMessageHandler<CombatText>
    {
        protected override async Awaitable InnerProcess(CombatText msg, CancellationToken token)
        {
            if (_objectManager.GetController(msg.TargetId, out UnitController controller))
            {
                controller.ShowCombatText(msg);
            }
            await Task.CompletedTask;
        }
    }
}




using Genrpg.Shared.Spells.Messages;
using System.Threading;
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
        }
    }
}



using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnRemoveEffectHandler : BaseClientMapMessageHandler<OnRemoveEffect>
    {
        protected override async Awaitable InnerProcess(OnRemoveEffect msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.TargetId, out Unit unit))
            {
                return;
            }

            _dispatcher.Dispatch(msg);
        }
    }
}



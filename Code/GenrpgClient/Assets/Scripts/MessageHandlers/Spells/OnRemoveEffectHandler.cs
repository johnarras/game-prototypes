using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;
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
            await Task.CompletedTask;
        }
    }
}



using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnAddEffectHandler : BaseClientMapMessageHandler<OnAddEffect>
    {
        protected override async Awaitable InnerProcess(OnAddEffect msg, CancellationToken token)
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



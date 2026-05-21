
using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.Targets.Messages;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Targets
{
    public class OnTargetIsDeadHandler : BaseClientMapMessageHandler<OnTargetIsDead>
    {
        protected override async Awaitable InnerProcess(OnTargetIsDead msg, CancellationToken token)
        {
            if (_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                unit.AddFlag(UnitFlags.IsDead);
            }
            if (_objectManager.GetController(msg.UnitId, out UnitController controller))
            {
                Died died = new Died()
                {
                    UnitId = msg.UnitId,
                };
                controller.OnDeath(died, token);
            }
            await Task.CompletedTask;
        }
    }
}



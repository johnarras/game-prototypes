using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Spawns
{
    public class DiedHandler : BaseClientMapMessageHandler<Died>
    {
        protected override async ValueTask InnerProcess(Died msg, CancellationToken token)
        {
            if (_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                unit.AddFlag(UnitFlags.IsDead);
                if (msg.FirstAttacker != null)
                {
                    unit.AddAttacker(msg.FirstAttacker.AttackerId, msg.FirstAttacker.GroupId);
                }
            }
            if (_objectManager.GetController(msg.UnitId, out UnitController controller))
            {
                controller.OnDeath(msg, token);
            }
            await Task.CompletedTask;
        }
    }
}



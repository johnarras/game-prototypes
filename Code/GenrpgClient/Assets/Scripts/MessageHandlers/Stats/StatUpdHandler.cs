using OxDb.SharedGame.Stats.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Stats
{
    public class StatUpdHandler : BaseClientMapMessageHandler<StatUpd>
    {
        protected override async ValueTask InnerProcess(StatUpd msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                return;
            }

            unit.Stats.UpdateFromSnapshot(msg.Dat);
            _dispatcher.Dispatch(msg);
            await Task.CompletedTask;
        }
    }
}



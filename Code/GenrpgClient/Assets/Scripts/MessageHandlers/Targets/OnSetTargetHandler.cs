
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Targets.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Targets
{
    public class OnSetTargetHandler : BaseClientMapMessageHandler<OnSetTarget>
    {
        protected override async ValueTask InnerProcess(OnSetTarget msg, CancellationToken token)
        {
            if (_objectManager.GetMapObject(msg.CasterId, out MapObject obj))
            {
                if (obj is Unit unit)
                {
                    unit.TargetId = msg.TargetId;
                }
            }
            await Task.CompletedTask;
        }
    }
}




using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Spawns
{
    public class DespawnObjectHandler : BaseClientMapMessageHandler<DespawnObject>
    {
        protected override async ValueTask InnerProcess(DespawnObject msg, CancellationToken token)
        {
            _objectManager.RemoveObject(msg.ObjId);
            await Task.CompletedTask;
        }
    }
}



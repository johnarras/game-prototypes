
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.Spawns
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



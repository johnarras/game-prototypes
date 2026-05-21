
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spawns
{
    public class DespawnObjectHandler : BaseClientMapMessageHandler<DespawnObject>
    {
        protected override async Awaitable InnerProcess(DespawnObject msg, CancellationToken token)
        {
            _objectManager.RemoveObject(msg.ObjId);
            await Task.CompletedTask;
        }
    }
}



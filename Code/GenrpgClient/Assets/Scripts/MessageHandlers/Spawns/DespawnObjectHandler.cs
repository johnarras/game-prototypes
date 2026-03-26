
using Genrpg.Shared.MapObjects.Messages;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spawns
{
    public class DespawnObjectHandler : BaseClientMapMessageHandler<DespawnObject>
    {
        protected override async Awaitable InnerProcess(DespawnObject msg, CancellationToken token)
        {
            _objectManager.RemoveObject(msg.ObjId);
        }
    }
}



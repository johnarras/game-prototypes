
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Targets
{
    public class OnSetTargetHandler : BaseClientMapMessageHandler<OnSetTarget>
    {
        protected override async Awaitable InnerProcess(OnSetTarget msg, CancellationToken token)
        {
            if (_objectManager.GetMapObject(msg.CasterId, out MapObject obj))
            {
                if (obj is Unit unit)
                {
                    unit.TargetId = msg.TargetId;
                }
            }
        }
    }
}



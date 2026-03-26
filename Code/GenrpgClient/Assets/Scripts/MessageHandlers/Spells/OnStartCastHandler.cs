using Genrpg.Shared.Spells.Messages;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnStartCastHandler : BaseClientMapMessageHandler<OnStartCast>
    {
        protected override async Awaitable InnerProcess(OnStartCast msg, CancellationToken token)
        {
            if (_objectManager.GetGridItem(msg.CasterId, out ClientMapObjectGridItem gridItem))
            {
                gridItem.Controller?.StartCasting(msg);
            }
        }
    }
}



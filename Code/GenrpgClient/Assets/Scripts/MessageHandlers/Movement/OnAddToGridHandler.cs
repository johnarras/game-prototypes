using Genrpg.Shared.Movement.Messages;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Movement
{
    public class OnAddToGridHandler : BaseClientMapMessageHandler<OnAddToGrid>
    {
        protected override async Awaitable InnerProcess(OnAddToGrid msg, CancellationToken token)
        {
            _objectManager.OnServerAddtoGrid(msg);
        }
    }
}



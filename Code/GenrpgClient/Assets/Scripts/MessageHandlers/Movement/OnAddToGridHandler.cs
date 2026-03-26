using Genrpg.Shared.Movement.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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



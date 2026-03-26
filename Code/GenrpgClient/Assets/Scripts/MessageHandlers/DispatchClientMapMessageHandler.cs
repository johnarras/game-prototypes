using Genrpg.Shared.MapMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers
{
    public abstract class DispatchClientMapMessageHandler<T> : BaseClientMapMessageHandler<T> where T : class, IMapApiMessage
    {
        protected override async Awaitable InnerProcess(T msg, CancellationToken token)
        {
            _dispatcher.Dispatch(msg);
        }
    }
}



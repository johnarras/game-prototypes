using Genrpg.Shared.MapMessages.Interfaces;
using System.Threading;
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



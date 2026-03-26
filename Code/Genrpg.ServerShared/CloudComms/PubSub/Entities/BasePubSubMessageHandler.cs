using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Entities
{
    public abstract class BasePubSubMessageHandler<M> : IPubSubMessageHandler where M : class, IPubSubMessage
    {
        public abstract Type HelperKey { get; }

        public async Task HandleMessage(IPubSubMessage message, CancellationToken token)
        {
            await InnerHandleMessage((M)message, token);
        }

        protected abstract Task InnerHandleMessage(M message, CancellationToken token);
    }
}



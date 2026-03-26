using Genrpg.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Queues.Entities
{
    public interface IQueueMessageHandler : ISetupDictionaryItem<Type>
    {
        Task HandleMessage(IQueueMessage message, CancellationToken token);
    }
}



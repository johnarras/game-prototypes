using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.CloudComms.Queues.Entities
{
    public interface IQueueMessageHandler : ISetupDictionaryItem<Type>
    {
        Task HandleMessage(IQueueMessage message, CancellationToken token);
    }
}



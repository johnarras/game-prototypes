using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.CloudComms.PubSub.Entities
{
    public interface IPubSubMessageHandler : ISetupDictionaryItem<Type>
    {
        Task HandleMessage(IPubSubMessage message, CancellationToken token);
    }
}



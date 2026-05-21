using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;

namespace OxDb.InstanceServer.MessageHandlers
{
    public class AddMapInstanceHandler : BaseInstanceMessageHandler<AddMapInstance>
    {
        protected override async Task InnerHandleMessage(AddMapInstance message)
        {
            _logService.Info("Received " + message.GetType().Name + " from " + message.ServerName);
            await _instanceManagerService.AddInstanceData(message);
            await Task.CompletedTask;
        }
    }
}



using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;

namespace OxDb.InstanceServer.MessageHandlers
{
    public class RemoveMapInstanceHandler : BaseInstanceMessageHandler<RemoveMapInstance>
    {

        protected override async Task InnerHandleMessage(RemoveMapInstance message)
        {
            _logService.Info("Received " + message.GetType().Name + " from " + message.FullInstanceId);
            await _instanceManagerService.RemoveInstanceData(message);
            await Task.CompletedTask;
        }
    }
}



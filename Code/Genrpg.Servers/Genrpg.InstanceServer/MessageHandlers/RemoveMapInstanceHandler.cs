using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class RemoveMapInstanceHandler : BaseInstanceMessageHandler<RemoveMapInstance>
    {

        protected override async Task InnerHandleMessage(RemoveMapInstance message)
        {
            _logService.Message("Received " + message.GetType().Name + " from " + message.FullInstanceId);
            await _instanceManagerService.RemoveInstanceData(message);
            await Task.CompletedTask;
        }
    }
}



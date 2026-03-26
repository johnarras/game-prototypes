using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class RemoveMapServerHandler : BaseInstanceMessageHandler<RemoveMapServer>
    {
        protected override async Task InnerHandleMessage(RemoveMapServer message)
        {
            _logService.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _instanceManagerService.RemoveMapServer(message);
            await Task.CompletedTask;
        }
    }
}



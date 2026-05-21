using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;

namespace OxDb.InstanceServer.MessageHandlers
{
    public class RemoveMapServerHandler : BaseInstanceMessageHandler<RemoveMapServer>
    {
        protected override async Task InnerHandleMessage(RemoveMapServer message)
        {
            _logService.Info("Received " + message.GetType().Name + " from " + message.ServerName);
            await _instanceManagerService.RemoveMapServer(message);
            await Task.CompletedTask;
        }
    }
}



using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;

namespace OxDb.InstanceServer.MessageHandlers
{
    public class AddMapServerHandler : BaseInstanceMessageHandler<AddMapServer>
    {
        protected override async Task InnerHandleMessage(AddMapServer message)
        {
            _logService.Info("Received " + message.GetType().Name + " from " + message.ServerName);
            await _instanceManagerService.AddMapServer(message);
            await Task.CompletedTask;
        }
    }
}



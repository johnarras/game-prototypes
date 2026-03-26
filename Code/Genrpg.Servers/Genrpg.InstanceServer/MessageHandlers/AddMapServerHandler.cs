using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class AddMapServerHandler : BaseInstanceMessageHandler<AddMapServer>
    {
        protected override async Task InnerHandleMessage(AddMapServer message)
        {
            _logService.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _instanceManagerService.AddMapServer(message);
            await Task.CompletedTask;
        }
    }
}



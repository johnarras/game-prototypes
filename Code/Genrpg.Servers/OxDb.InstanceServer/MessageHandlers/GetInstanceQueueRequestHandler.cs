using OxDb.InstanceServer.Entities;
using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;
using OxDb.ServerCore.CloudComms.Servers.WebServer;
using OxDb.ServerCore.CloudComms.Services;

namespace OxDb.InstanceServer.MessageHandlers
{
    public class GetInstanceQueueRequestHandler : BaseInstanceMessageHandler<GetInstanceQueueRequest>
    {

        private ICloudCommsService _commsService = null;

        protected override async Task InnerHandleMessage(GetInstanceQueueRequest message)
        {
            MapInstanceData instanceData = await _instanceManagerService.GetInstanceDataForMap(message.MapId);
            _logService.Info("Instance Data for mapId: " + message.MapId + " is " + instanceData);
            GetInstanceQueueResponse response = new GetInstanceQueueResponse()
            {
                RequestId = message.RequestId,
            };

            if (instanceData == null)
            {
                response.ErrorText = "Missing Map Instance " + message.MapId;
            }
            else
            {
                response.Host = instanceData.Host;
                response.Port = instanceData.Port;
                response.InstanceId = instanceData.InstanceId;
                response.SerializerType = instanceData.SerializerType;
            }
            _commsService.SendQueueMessage(message.FromServerName, response);
        }
    }
}



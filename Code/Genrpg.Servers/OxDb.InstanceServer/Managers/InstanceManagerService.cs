using OxDb.InstanceServer.Entities;
using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;

namespace OxDb.InstanceServer.Managers
{
    public interface IInstanceManagerService : IInjectable
    {
        Task AddInstanceData(AddMapInstance mapInstance);
        Task<MapInstanceData> GetInstanceDataForMap(string mapId);
        Task RemoveInstanceData(RemoveMapInstance removeInstance);

        Task AddMapServer(AddMapServer mapServer);
        Task<MapServerData> GetServerData(string mapServerName);
        Task RemoveMapServer(RemoveMapServer removeMapServer);
    }

    public class InstanceManagerService : IInstanceManagerService
    {

        private List<MapInstanceData> _mapInstances = new List<MapInstanceData>();

        private List<MapServerData> _mapServers = new List<MapServerData>();

        private ILogService _logService = null;
        public async Task AddInstanceData(AddMapInstance addMapInstance)
        {
            _logService.Info("Add Instance " + addMapInstance.MapId + " Host: " + addMapInstance.Host + " Port: " + addMapInstance.Port);

            MapInstanceData instanceData = new MapInstanceData()
            {
                Host = addMapInstance.Host,
                InstanceId = addMapInstance.InstanceId,
                MapId = addMapInstance.MapId,
                Port = addMapInstance.Port,
                ServerName = addMapInstance.ServerName,
                Size = addMapInstance.Size,
                SerializerType = addMapInstance.SerializerType,
            };

            _mapInstances.Add(instanceData);

            await Task.CompletedTask;
        }

        public async Task<MapInstanceData> GetInstanceDataForMap(string mapId)
        {
            await Task.CompletedTask;

            foreach (MapInstanceData mid in _mapInstances)
            {
                _logService.Info("MapInstance: " + mid.MapId + " need " + mapId);
            }

            return _mapInstances.FirstOrDefault(x => x.MapId == mapId);
        }

        public async Task RemoveInstanceData(RemoveMapInstance removeInstance)
        {
            _logService.Info("Remove Instance " + removeInstance.FullInstanceId);

            _mapInstances = _mapInstances.Where(x => x.InstanceId != removeInstance.FullInstanceId).ToList();
            await Task.CompletedTask;
        }

        public async Task AddMapServer(AddMapServer mapServer)
        {
            MapServerData serverData = new MapServerData()
            {
                MapServerName = mapServer.ServerName,
            };
            _mapServers.Add(serverData);

            await Task.CompletedTask;
        }

        public async Task<MapServerData> GetServerData(string mapServerName)
        {
            await Task.CompletedTask;
            return _mapServers.FirstOrDefault(x => x.MapServerName == mapServerName);
        }

        public async Task RemoveMapServer(RemoveMapServer removeMapServer)
        {
            _mapServers = _mapServers.Where(x => x.MapServerName != removeMapServer.ServerName).ToList();
            await Task.CompletedTask;
        }
    }
}



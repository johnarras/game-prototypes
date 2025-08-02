using Genrpg.InstanceServer.Entities;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using MongoDB.Driver.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.Managers
{
    public interface IInstanceManagerService : IInjectable
    {
        Task AddInstanceData(AddMapInstance mapInstance);
        Task<MapInstanceData> GetInstanceDataForMap(string mapId);
        Task RemoveInstanceData(RemoveMapInstance removeInstance);

        Task AddMapServer(AddMapServer mapServer);
        Task<MapServerData> GetServerData(string mapServerId);
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
                ServerId = addMapInstance.ServerId,
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
                MapServerId = mapServer.ServerId,
            };
            _mapServers.Add(serverData);

            await Task.CompletedTask;
        }

        public async Task<MapServerData> GetServerData(string mapServerId)
        {
            await Task.CompletedTask;
            return _mapServers.FirstOrDefault(x => x.MapServerId == mapServerId);
        }

        public async Task RemoveMapServer(RemoveMapServer removeMapServer)
        {
            _mapServers = _mapServers.Where(x => x.MapServerId != removeMapServer.ServerId).ToList();
            await Task.CompletedTask;
        }
    }
}

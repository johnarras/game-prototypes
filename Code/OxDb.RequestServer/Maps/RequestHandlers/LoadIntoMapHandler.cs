using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.Purchasing.Services;
using OxDb.ServerCore.CloudComms.Servers.InstanceServer.Queues;
using OxDb.ServerCore.CloudComms.Servers.WebServer;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.ServerGame.Maps;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.MapServer.Entities.MapCache;
using OxDb.SharedGame.MapServer.WebApi.LoadIntoMap;
using OxDb.SharedGame.Networking.Constants;
using OxDb.SharedGame.Purchasing.PlayerData;
using System.Collections.Concurrent;

namespace OxDb.RequestServer.Maps.RequestHandlers
{
    public class LoadIntoMapHandler : BaseClientUserRequestHandler<LoadIntoMapRequest>
    {
        private IServerGameDataService _gameDataService = null;
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;
        private IServerPurchasingService _purchasingService = null;

        private ConcurrentDictionary<string, CachedMap> _mapCache = new ConcurrentDictionary<string, CachedMap>();
        public override async Task Reset()
        {
            _mapCache = new ConcurrentDictionary<string, CachedMap>();
            await Task.CompletedTask;
        }

        protected override async Task InnerHandleMessage(WebContext context, LoadIntoMapRequest request, CancellationToken token)
        {
            FullCachedMap fullCachedMap = await GetCachedMap(context, request.Env, request.MapId, request.InstanceId, request.GenerateMap);

            // Check case where the map doesn't exist, if not create that map.
            if (fullCachedMap == null || fullCachedMap.Map == null ||
                fullCachedMap.Map.Zones == null)
            {
                long mapId = -1;
                long.TryParse(request.MapId, out mapId);
                if (request.GenerateMap)
                {
                    fullCachedMap.Map = new Map() { Id = request.MapId };
                }
                else
                {
                    ShowError(context, "Couldn't find map: " + request.MapId);
                    return;
                }
            }

            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);
            if (coreCh == null)
            {
                ShowError(context, "Couldn't find new character to load " + request.CharId);
                return;
            }

            if (coreCh.UserId != context.GameUserId)
            {
                ShowError(context, "You don't own this character");
                return;
            }
            if (coreCh.MapId != request.MapId)
            {
                context.Set(coreCh);
                coreCh.X = fullCachedMap.Map.SpawnX;
                coreCh.Z = fullCachedMap.Map.SpawnY;
            }

            Character ch = new Character(coreCh);

            List<IUnitData> serverDataList = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, ch, null);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context, ch, true, token);

            List<IUnitData> clientDataList = await _playerDataService.MapToClientDto(await context.GetAsync<CoreData>(), serverDataList);

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            string worldDataEnv = _config.DataEnvs[EDataCategories.Worlds.ToString()];

            if (request.GenerateMap && !string.IsNullOrEmpty(request.WorldDataEnv))
            {
                worldDataEnv = request.WorldDataEnv;
            }

            try
            {
                context.AddResponseRange(_gameDataService.GetClientSettings(ch, true));
                LoadIntoMapResponse loadResponse = new LoadIntoMapResponse()
                {
                    Map = _serializer.ConvertType<Map, Map>(fullCachedMap.Map),
                    Generating = request.GenerateMap,
                    Char = coreCh,
                    Host = fullCachedMap.MapInstance?.Host,
                    Port = fullCachedMap.MapInstance?.Port ?? 0,
                    Serializer = fullCachedMap?.MapInstance?.SerializerType ?? EMapApiSerializers.Json,
                    CharData = clientDataList,
                    WorldDataEnv = worldDataEnv,
                    Stores = offerData,
                };

                GameAccount acct = await context.GetAsync<GameAccount>();
                acct.CurrCharId = coreCh.Id;

                context.AddResponse(loadResponse);

                PublicCharacter publicChar = new PublicCharacter()
                {
                    Id = coreCh.Id,
                    DisplayName = coreCh.Name,
                    FactionTypeId = coreCh.FactionTypeId,
                    SexTypeId = coreCh.SexTypeId,
                    UnitTypeId = coreCh.EntityId
                };

                await _repoService.Save(publicChar);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "LoadIntoMap");
            }
        }
        // This needs to be sent to another server someplace to handle this synchronization and load balancing.
        private async Task<FullCachedMap> GetCachedMap(WebContext context, string env, string mapId, string instanceId, bool generatingMap)
        {
            if (!_mapCache.ContainsKey(mapId))
            {
                Map newMap = await _mapDataService.LoadMap(context.Rand, mapId);
                if (newMap == null || newMap.Zones == null || newMap.Zones.Count < 1)
                {
                    return new FullCachedMap();
                }
                CachedMap newCachedMap = new CachedMap()
                {
                    FullMap = newMap,
                };

                Map clientMap = _serializer.MakeCopy(newMap);
                clientMap.CleanForClient();
                newCachedMap.ClientMap = clientMap;

                _mapCache.TryAdd(mapId, newCachedMap);
            }

            GetInstanceQueueResponse response = await _cloudCommsService.SendResponseMessageAsync<GetInstanceQueueResponse>(ServerNames.InstanceManager, new GetInstanceQueueRequest() { MapId = mapId });

            if (!generatingMap && (response == null || !string.IsNullOrEmpty(response.ErrorText)))
            {
                return new FullCachedMap();
            }

            CachedMap cachedMap = _mapCache[mapId];

            FullCachedMap fullMap = new FullCachedMap()
            {
                Map = generatingMap ? cachedMap.FullMap : cachedMap.ClientMap,
            };

            if (!generatingMap)
            {
                fullMap.MapInstance = new CachedMapInstance()
                {
                    Host = response.Host,
                    Port = response.Port,
                    InstanceId = response.InstanceId,
                    SerializerType = response.SerializerType,
                };
            }

            return fullMap;
        }

    }
}



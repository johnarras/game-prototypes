using OxDb.Client.FileUploads;
using OxDb.Client.Networking.Services;
using OxDb.Client.Repository.Constants;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.MapServer.WebApi.UploadMap;
using System.Threading;
using UnityEngine;

public class UploadMap : BaseZoneGenerator
{
    private IClientWebRequestService _webNetworkService = null;
    private IClientAppService _appService = null;
    private IClientConfigContainer _configContainer = null;
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);

        Map map = _mapProvider.GetMap();

        string subfolder = MapUtils.GetMapFolder(map.Id, map.MapVersion);
        string localPath = _appService.PersistentDataPath + ClientRepositoryConstants.GetDataPathPrefix() + "/" + subfolder;

        FolderUploadArgs uploadData = new FolderUploadArgs()
        {
            LocalFolder = localPath,
            RemoteSubfolder = subfolder,
            IsWorldData = true,
            Env = _assetService.GetWorldDataEnv(),
            GamePrefix = _configContainer.Config.GameMode.ToString(),
        };

        uploadData.FilePatterns.Add("*");

        await FileUploader.UploadFolder(uploadData, "MapMap.jpg");

        await DelaySendMapSizes(token);
    }


    private async Awaitable DelaySendMapSizes(CancellationToken token)
    {
        await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
        UploadMapRequest update = new UploadMapRequest()
        {
            Map = _mapProvider.GetMap(),
            SpawnData = _mapProvider.GetSpawns(),
        };

        string oldMapId = _mapProvider.GetMap().Id;
        _mapProvider.GetMap().Id = "UploadedMap";
        await _clientRepoService.Save(_mapProvider.GetMap());
        _mapProvider.GetMap().Id = oldMapId;
        _mapProvider.GetSpawns().Id = "UploadedSpawns";
        await _clientRepoService.Save(_mapProvider.GetSpawns());
        _mapProvider.GetSpawns().Id = oldMapId;
        _webNetworkService.SendMainServerRequest(update, _token);

    }
}




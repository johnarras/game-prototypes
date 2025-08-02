using Assets.Scripts.MapTerrain;
using Assets.Scripts.Repository.Constants;
using Genrpg.Shared.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using NUnit;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class UploadMap : BaseZoneGenerator
{
    private IClientWebService _webNetworkService = null;
    private IClientAppService _appService = null;
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
            GamePrefix = Game.Prefix,
        };

        FileUploader.UploadFolder(uploadData);

        await DelaySendMapSizes(token);
    }


    private async Awaitable DelaySendMapSizes(CancellationToken token)
    {
        await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
        UploadMapRequest update = new UploadMapRequest()
        {
            Map = _mapProvider.GetMap(),
            SpawnData = _mapProvider.GetSpawns(),
            WorldDataEnv = _assetService.GetWorldDataEnv()
        };

        string oldMapId = _mapProvider.GetMap().Id;
        _mapProvider.GetMap().Id = "UploadedMap";
        await _repoService.Save(_mapProvider.GetMap());
        _mapProvider.GetMap().Id = oldMapId;
        _mapProvider.GetSpawns().Id = "UploadedSpawns";
        await _repoService.Save(_mapProvider.GetSpawns());
        _mapProvider.GetSpawns().Id = oldMapId;
        _webNetworkService.SendClientUserWebRequest(update, _token);
        
    }
}
	

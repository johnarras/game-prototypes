using Assets.Scripts.Minimap.Services;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedGame.MapServer.Entities;
using System;
using System.Threading;
using UnityEngine;

public class LoadMinimap : BaseZoneGenerator
{
    private IMinimapService _minimapService = null;
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        try
        {
            string filename = MapUtils.GetMapObjectFilename(MapConstants.MapFilename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
            byte[] bytes = _clientRepoService.LoadBytes(filename);
            if (bytes != null)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                OnDownloadMinimap(tex, null, token);
            }
            else
            {
                DownloadFileData ddata = new DownloadFileData()
                {
                    IsImage = true,
                    Handler = OnDownloadMinimap,
                    Category = EDataCategories.Worlds,
                };
                _fileDownloadService.DownloadFile(filename, ddata, token);
            }
        }
        catch (Exception e)
        {
            _logService.Exception(e, "LoadMinimap");
        }
    }

    private void OnDownloadMinimap(object obj, object data, CancellationToken token)
    {
        Texture2D tex = obj as Texture2D;

        _minimapService.SetTexture(tex);
    }
}



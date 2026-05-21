using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Pathfinding.Constants;
using System.Threading;
using UnityEngine;

public class LoadPathfinding : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        string filename = MapUtils.GetMapObjectFilename(PathfindingConstants.Filename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
        byte[] bytes = _clientRepoService.LoadBytes(filename);
        if (bytes != null)
        {
            OnDownloadPathfinding(bytes, null, token);
        }
        else
        {
            DownloadFileData ddata = new DownloadFileData()
            {
                IsImage = false,
                Handler = OnDownloadPathfinding,
                Category = EDataCategories.Worlds,
            };
            _fileDownloadService.DownloadFile(filename, ddata, token);
        }
    }

    private void OnDownloadPathfinding(object obj, object data, CancellationToken token)
    {

        byte[] compressedBytes = obj as byte[];

        if (compressedBytes == null)
        {
            return;
        }
        byte[] decompressedBytes = CompressionUtils.DecompressBytes(compressedBytes);

        _pathfindingService.SetPathfinding(_pathfindingService.ConvertBytesToGrid(decompressedBytes));

    }
}



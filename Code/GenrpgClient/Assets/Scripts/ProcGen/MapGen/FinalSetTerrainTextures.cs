using OxDb.Client.MapTerrain;
using OxDb.SharedGame.ProcGen.Constants;
using System.Threading;
using UnityEngine; // Needed

public class SetFinalTerrainTextures : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        _mapProvider.GetMap().OverrideZonePercent = 0;
        _zoneGenService.SetAllAlphamaps(_md.Alphas, token);
        await WaitForTerrainLayerLoad(token);
    }

    private async Awaitable WaitForTerrainLayerLoad(CancellationToken token)
    {
        for (int x = 0; x < _md.Awid; x++)
        {
            for (int z = 0; z < _md.Ahgt; z++)
            {
                float total = 0;
                for (int i = 0; i < TerrainTexChannels.Max; i++)
                {
                    total += _md.Alphas[x, z, i];
                }
                if (total < 0.1f)
                {
                    _md.ClearAlphasAt(x, z);
                    _md.Alphas[x, z, TerrainTexChannels.Base] = 1.0f;
                }
                else
                {
                    for (int i = 0; i < TerrainTexChannels.Max; i++)
                    {
                        _md.Alphas[x, z, i] /= total;
                    }
                }
            }
        }

        while (true)
        {
            if (_assetService.IsDownloading())
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            else
            {
                break;
            }
        }

        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);

        while (true)
        {
            bool missingLayer = false;

            for (int x = 0; x < _mapProvider.GetMap().BlockCount; x++)
            {
                if (missingLayer)
                {
                    break;
                }
                for (int z = 0; z < _mapProvider.GetMap().BlockCount; z++)
                {
                    if (missingLayer)
                    {
                        break;
                    }

                    TerrainPatchData patch = _terrainManager.GetTerrainPatch(x, z);

                    if (patch == null || !patch.Core.IsReady())
                    {
                        missingLayer = true;
                        break;
                    }

                    TerrainData tdata = _terrainManager.GetTerrainData(x, z);
                    if (tdata == null)
                    {
                        missingLayer = true;
                        break;
                    }
                    TerrainLayer[] layers = tdata.terrainLayers;
                    if (layers == null || layers.Length < 1)
                    {
                        missingLayer = true;
                        break;
                    }

                    for (int s = 0; s < layers.Length; s++)
                    {
                        if (layers[s] == null || layers[s].diffuseTexture == null)
                        {
                            missingLayer = true;
                            break;
                        }
                    }

                    if (!missingLayer && !patch.HaveSetAlphamaps)
                    {
                        missingLayer = true;
                        break;
                    }
                }
            }

            if (missingLayer)
            {
                await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
            }
            else
            {
                break;
            }
        }

        while (true)
        {
            if (_assetService.IsDownloading())
            {
                await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
            }
            else
            {
                break;
            }
        }


        await Awaitable.WaitForSecondsAsync(10.0f, cancellationToken: token);

        _md.HaveSetAlphaSplats = true;
    }
}




using Assets.Scripts.MapTerrain;
using Assets.Scripts.Minimap.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CreateMinimap : BaseZoneGenerator
{
    public const int TexSize = 4096;
    public const string CreateMinimapCamera = "CreateMinimapCamera";
    private GameObject minimapCamera = null;

    private IZoneStateController _zoneStateController = null;
    private IMinimapService _minimapService = null;

    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        _clientEntityService.Destroy(minimapCamera);

        minimapCamera = new GameObject();
        minimapCamera.name = "CreateMinimapCamera";
        minimapCamera.AddComponent<Camera>();

        minimapCamera.SetActive(true);
        Camera cam = minimapCamera.GetComponent<Camera>();
        cam.farClipPlane = 5000;
        cam.allowMSAA = false;
        cam.allowHDR = false;
        cam.useOcclusionCulling = false;

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed + 44235);

        int zoneMapSize = (int)(_mapProvider.GetMap().GetHwid());
        List<float[,]> noiseList = new List<float[,]>();


        // Make noise for each of the 3 channels.

        for (int i = 0; i < 3; i++)
        {
            float amp = RandUtils.FloatRange(0.1f, 0.2f, rand) * 0.4f;
            float freq = RandUtils.FloatRange(0.05f, 0.20f, rand) * TexSize * 1.2f;
            float pers = RandUtils.FloatRange(0.15f, 0.3f, rand);
            int octaves = 2;
            float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), TexSize, TexSize);
            noiseList.Add(noise);
        }


        cam.orthographic = true;
        cam.orthographicSize = zoneMapSize / 2;
        cam.transform.position = new Vector3(zoneMapSize / 2, MapConstants.MapHeight * 2, zoneMapSize / 2);
        cam.transform.LookAt(new Vector3(zoneMapSize / 2, 0, zoneMapSize / 2));
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.renderingPath = RenderingPath.DeferredShading;
        cam.cullingMask = -1;

        UnityEngine.Color ambientColor = RenderSettings.ambientSkyColor;
        float ambientScale = 0.6f;
        RenderSettings.ambientLight = new Color(ambientScale, ambientScale, ambientScale, 1);

        List<string> sunNames = new List<String>() { "Sun", "Sunlight" };

        Light light = _zoneStateController.SunLight;

        float lightIntensity = 1.0f;
        UnityEngine.Color sunColor = Color.white;
        Vector3 oldAngles = new Vector3(90, 0, 0);
        if (light != null)
        {
            lightIntensity = light.intensity;
            light.intensity = 0.9f;
            sunColor = light.color;
            light.color = new Color(1.0f, 0.95f, 0.9f);
            oldAngles = light.transform.localEulerAngles;
            light.transform.localEulerAngles = new Vector3(80, 0, 0);
        }

        float waterRed = 0.5f;
        float waterGreen = 0.6f;
        float waterBlue = 0.7f;


        RenderSettings.fog = false;
        // All terrain splats should have been loaded during SetFinalTerrainTextures.

        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);

        int numTerrainsNeeded = _mapProvider.GetMap().BlockCount * _mapProvider.GetMap().BlockCount;

        List<Terrain> terrains = _terrainManager.GetTerrains();

        while (terrains.Count < numTerrainsNeeded)
        {
            await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: _token);
        }

        while (_terrainManager.IsLoadingPatches())
        {
            await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: _token);
        }

        float oldBasemapDist = 250;
        float oldPixelError = 5;
        int oldLOD = 0;

        foreach (Terrain terr in terrains)
        {
            oldBasemapDist = terr.basemapDistance;
            terr.basemapDistance = 250;
            oldPixelError = terr.heightmapPixelError;
            terr.heightmapPixelError = 3;
            oldLOD = terr.heightmapMaximumLOD;
            terr.heightmapMaximumLOD = 0;
        }

        GameObject waterRoot = new GameObject();
        waterRoot.name = "WaterRoot";
        TerrainPatchData patch = _terrainManager.GetTerrainPatch(0, 0);
        await Awaitable.NextFrameAsync(cancellationToken: token);

        IMMOMapObjectLoader waterLoader = _terrainManager.GetLoader(EntityTypes.Water);

        List<ExtendedWorldObjectData> waterObjects = new List<ExtendedWorldObjectData>();

        for (int x = 0; x < _md.ExtendedObjects.GetLength(0); x++)
        {
            for (int z = 0; z < _md.ExtendedObjects.GetLength(1); z++)
            {
                if (_md.ExtendedObjects[x, z] != null && _md.ExtendedObjects[x, z].EntityTypeId == EntityTypes.Water)
                {
                    waterObjects.Add(_md.ExtendedObjects[x, z]);
                }
            }
        }

        waterObjects.Add(new ExtendedWorldObjectData()
        {
            EntityTypeId = EntityTypes.Water,
            EntityId = 0,
            X = _mapProvider.GetMap().GetHwid() / 2,
            Z = _mapProvider.GetMap().GetHhgt() / 2,
            XSize = 100000,
            ZSize = 100000,
            Height = -5,
        });

        patch.ExtendedObjects = waterObjects;

        int waterObjectCount = 0;
        foreach (ExtendedWorldObjectData extWater in waterObjects)
        {
            int x = (int)extWater.X;
            int z = (int)extWater.Z;

            int nx = x + 0 * (x / (MapConstants.TerrainPatchSize - 1));
            int nz = z + 0 * (z / (MapConstants.TerrainPatchSize - 1));

            PatchLoadData loadData = new PatchLoadData()
            {
                gx = 0,
                gz = 0,
                StartX = 0,
                StartZ = 0,
                protoParent = waterRoot,
                patch = patch,
            };

            waterObjectCount++;
            waterLoader.LoadObject(loadData, 0, nz, nx, null, null, token);
        }

        await Awaitable.WaitForSecondsAsync(0.1f * waterObjectCount, cancellationToken: token);

        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGB24, true, true);

        RenderTexture rt = new RenderTexture(TexSize, TexSize, 24);
        //rt.anisoLevel = 3;
        //rt.antiAliasing = 4;
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        cam.Render();
        tex.ReadPixels(new Rect(0, 0, TexSize, TexSize), 0, 0);
        tex.Apply();

        await Awaitable.NextFrameAsync(cancellationToken: token);

        UnityEngine.Color[] pixels = tex.GetPixels();

        bool[,] bluePixels = new bool[TexSize, TexSize];

        float colorDelta = 16.0f / 256f;

        for (int x = 0; x < TexSize; x++)
        {
            for (int z = 0; z < TexSize; z++)
            {
                UnityEngine.Color pix = pixels[GetIndex(x, z)];
                if (pix.r < colorDelta &&
                    pix.g < colorDelta &&
                    pix.b > 1 - colorDelta)
                {
                    bluePixels[x, z] = true;
                }
            }
        }
        float[] tint = new float[3];
        tint[0] = 0.75f;
        tint[1] = 0.5f;
        tint[2] = 0.25f;

        float contrast = 1.15f;
        float bright = -0.05f;

        float[] newColor = new float[3];

        float tintPct = 0.05f;

        int rad = 3;

        for (int x = 0; x < TexSize; x++)
        {
            for (int z = 0; z < TexSize; z++)
            {
                if (bluePixels[x, z])
                {
                    float minDistToOther = rad * 3;
                    for (int xx = x - rad; xx <= x + rad; xx++)
                    {
                        if (xx < 0 || xx >= TexSize)
                        {
                            continue;
                        }
                        int dx = xx - x;
                        for (int zz = z - rad; zz <= z + rad; zz++)
                        {
                            if (zz < 0 || zz >= TexSize)
                            {
                                continue;
                            }

                            if (!bluePixels[xx, zz])
                            {
                                int dz = zz - z;
                                float dist = (float)Math.Sqrt(dx * dx + dz * dz);
                                if (dist < minDistToOther)
                                {
                                    minDistToOther = dist;
                                }
                            }
                        }
                    }
                    if (minDistToOther > rad + 1)
                    {
                        minDistToOther = rad + 1;
                    }

                    float distPct = (minDistToOther - 1) / rad;
                    float startVal = 1.0f;
                    float lossPct = 0.1f;
                    newColor[0] = (startVal * waterRed) * (1 + distPct * lossPct);
                    newColor[1] = (startVal * waterGreen) * (1 + distPct * lossPct);
                    newColor[2] = (startVal * waterBlue) * (1 + distPct * lossPct);

                    pixels[GetIndex(x, z)] = new Color(newColor[0], newColor[1], newColor[2]);
                }
                else
                {
                    // move toward target tint.
                    UnityEngine.Color pcolor = pixels[GetIndex(x, z)];
                    newColor[0] = pcolor.r;
                    newColor[1] = pcolor.g;
                    newColor[2] = pcolor.b;
                    for (int i = 0; i < 3; i++)
                    {
                        newColor[i] = (1 - tintPct) * newColor[i] + tintPct * tint[i];
                    }
                    pixels[GetIndex(x, z)] = new Color(newColor[0], newColor[1], newColor[2]);
                }
            }
        }


        float smoothDivisor = 12.2f;
        UnityEngine.Color[] smoothingPixels = new UnityEngine.Color[pixels.Length];
        Array.Copy(pixels, smoothingPixels, pixels.Length);
        double[] totals = new double[3];
        for (int x = 0; x < TexSize; x++)
        {
            for (int z = 0; z < TexSize; z++)
            {
                double totalWeightNearby = 0;
                for (int i = 0; i < 3; i++)
                {
                    totals[i] = 0;
                }
                for (int xx = x - 1; xx <= x + 1; xx++)
                {
                    if (xx < 0 || xx >= TexSize)
                    {
                        continue;
                    }
                    float dx = Math.Abs(x - xx);
                    for (int zz = z - 1; zz <= z + 1; zz++)
                    {
                        if (zz < 0 || zz >= TexSize)
                        {
                            continue;
                        }
                        float dz = Math.Abs(z - zz);
                        float scaleFactor = 1.0f / (1 + smoothDivisor * (dx + dz + dx * dz));
                        UnityEngine.Color pix = smoothingPixels[GetIndex(xx, zz)];

                        totals[0] += pix.r * scaleFactor;
                        totals[1] += pix.g * scaleFactor;
                        totals[2] += pix.b * scaleFactor;
                        totalWeightNearby += scaleFactor;
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    totals[i] /= totalWeightNearby;
                    totals[i] = totals[i] * (1.0f + noiseList[i][x, z]);
                    totals[i] = Math.Max(0, contrast * (totals[i] - 0.5f) + 0.5f + bright);
                }

                pixels[GetIndex(x, z)] = new Color((float)totals[0], (float)totals[1], (float)totals[2]);
            }
        }

        await Awaitable.NextFrameAsync(cancellationToken: token);

        float minLandHeight = MapConstants.OceanHeight;
        float minLandPct = (minLandHeight) / MapConstants.MapHeight;

        int blackBorderWidth = 2;

        UnityEngine.Color waterColor = new Color(waterRed, waterGreen, waterBlue);

        float shiftScale = (MapConstants.TerrainPatchSize - 1) * 1.0f / (MapConstants.TerrainPatchSize - 0);
        float darkenStartPercent = 0.30f;
        float blendHigherLandDist = 30;
        for (int x = 0; x < TexSize; x++)
        {
            int xpos = (int)((shiftScale * 1.0 * x / TexSize) * _mapProvider.GetMap().GetHwid());
            for (int z = 0; z < TexSize; z++)
            {
                int zpos = (int)((shiftScale * 1.0 * z / TexSize) * _mapProvider.GetMap().GetHhgt());

                float belowMinLandDist = minLandPct - _terrainManager.GetInterpolatedHeight(xpos, zpos) / MapConstants.MapHeight;

                if (belowMinLandDist > 0)
                {
                    float origBelowLandDist = belowMinLandDist;

                    belowMinLandDist *= 40;
                    if (belowMinLandDist > 0.90f)
                    {
                        belowMinLandDist = 0.90f;
                    }

                    UnityEngine.Color color = waterColor * belowMinLandDist + UnityEngine.Color.white * (1 - belowMinLandDist) * 0.9f;

                    if (origBelowLandDist < blendHigherLandDist)
                    {
                        float oldColorPct = origBelowLandDist / blendHigherLandDist;
                        color = pixels[GetIndex(x, z)] * oldColorPct + color * (1 - oldColorPct);
                    }
                    else if (belowMinLandDist > darkenStartPercent)
                    {
                        float pctLeft = (1.0f - belowMinLandDist) / (1.0f - darkenStartPercent);

                        color *= pctLeft;
                    }
                    pixels[GetIndex(x, z)] = color;
                }
                if (x < blackBorderWidth || z < blackBorderWidth || x >= TexSize - blackBorderWidth || z >= TexSize - blackBorderWidth)
                {
                    pixels[GetIndex(x, z)] = waterColor;
                }
            }
        }

        cam.targetTexture = null;
        RenderTexture.active = null;
        _clientEntityService.Destroy(rt);
        _clientEntityService.Destroy(minimapCamera);
        minimapCamera = null;

        RenderSettings.ambientSkyColor = ambientColor;
        RenderSettings.ambientEquatorColor = ambientColor;
        RenderSettings.ambientGroundColor = ambientColor;
        if (light != null)
        {
            light.intensity = lightIntensity;
            light.color = sunColor;
            light.transform.localEulerAngles = oldAngles;
        }

        foreach (Terrain terr in terrains)
        {
            terr.basemapDistance = oldBasemapDist;
            terr.heightmapPixelError = oldPixelError;
            terr.heightmapMaximumLOD = oldLOD;
        }


        tex.SetPixels(pixels);

        string filename = MapUtils.GetMapObjectFilename(MapConstants.MapFilename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
        await _clientRepoService.SaveBytes(filename, tex.EncodeToJPG(100));

        _clientEntityService.DestroyAllChildren(waterRoot);
        _minimapService.SetTexture(tex);

        RenderSettings.fog = true;
    }

    protected int GetIndex(int x, int z)
    {
        return x + z * TexSize;
    }
}




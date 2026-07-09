using Assets.Scripts.Assets.Constants;
using Assets.Scripts.ClientEvents.DataUpdates;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.MapTerrain;
using Assets.Scripts.Minimap.Services;
using Assets.Scripts.Setup.Interfaces;
using ClientEvents;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Characters.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.MapServer.WebApi.LoadIntoMap;
using OxDb.SharedGame.ProcGen.Constants;
using OxDb.SharedGame.Spawns.WorldData;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedGame.Zones.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine; // Needed

public class UnityZoneGenService : ZoneGenService
{
    public const string LoadMapURLSuffix = "/LoadMap";

    public const float ObjectScale = 1.0f;

    protected IScreenService _screenService = null;
    protected IMapTerrainManager _terrainManager = null;
    private IClientWebService _webNetworkService = null;
    private IRealtimeNetworkService _networkService = null;
    protected ITextSerializer _serializer = null;

    private ITerrainTextureManager _textureManager = null;

    private CancellationTokenSource _mapTokenSource = null;
    private CancellationToken _mapToken;
    private CancellationToken _gameToken;
    private IAssetService _assetService = null;
    private IMinimapService _minimapService = null;
    private IClientAppService _appService = null;

    public override void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    public override void CancelMapToken()
    {
        _mapTokenSource?.Cancel();
        _mapTokenSource?.Dispose();
        _mapTokenSource = null;
    }

    public override void InstantiateMap(string worldId)
    {
        _mapTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_gameToken);
        _mapToken = _mapTokenSource.Token;
        foreach (IMapTokenService service in _gs.loc.GetVals<IMapTokenService>())
        {
            service.SetMapToken(_mapToken);
        }
        _awaitableService.ForgetAwaitable(InnerGenerate(worldId, _mapToken));
    }

    protected async Awaitable InnerGenerate(string worldId, CancellationToken token)
    {
        if (_md.GeneratingMap)
        {
            return;
        }

        _dispatcher.Dispatch(new CloseAllScreens());
        _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading));
        await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken: token);
        _md.GeneratingMap = true;
        RenderSettings.fog = false;
        await Awaitable.NextFrameAsync(cancellationToken: token);
        // Now carry out the actual generation steps
        List<IZoneGenerator> genlist = new List<IZoneGenerator>();

        if (string.IsNullOrEmpty(LoadedMapId))
        {
            genlist.Add(new ClearMapData());

            genlist.Add(new GenerateMap());

            genlist.Add(new SetupMapData());

            genlist.Add(new SetBasicTerrainTextures());

            genlist.Add(new SetBaseTerrainHeights());

            genlist.Add(new AddZoneNoise());

            genlist.Add(new SetupMountainDecayPower());

            // Get the centerpoints
            genlist.Add(new AddZoneCenters());

            // Connect the zone centers with roads.
            genlist.Add(new ConnectZoneCenters());

            // Setup road distances first time so we know where these roads are.
            genlist.Add(new SetupRoadDistances());

            // Add secondary locations all over the map, not too close to caves or other locations.
            genlist.Add(new AddSecondaryLocations());

            // Connect secondary locations to nearest? road.
            genlist.Add(new ConnectSecondaryLocations());

            genlist.Add(new SetupRoadDistances());

            genlist.Add(new CreateConnectedZones());

            genlist.Add(new AddEdgeMountains());

            genlist.Add(new SetupNearbyZones());

            genlist.Add(new AddMiddleMountains());

            genlist.Add(new AddMountainNoise());

            genlist.Add(new SetMountainHeights());

            genlist.Add(new RemoveSetupZonePatches());

            genlist.Add(new SetupRoadDistances());

            genlist.Add(new SetupOverrideTerrainPatches());

            genlist.Add(new AddOutcroppings());

            genlist.Add(new AddCrevices());

            genlist.Add(new AddDetailHeights());

            genlist.Add(new AddRoadDips());

            genlist.Add(new RaiseOrLowerZones());

            genlist.Add(new AddOceans());

            genlist.Add(new AddLocationPatches());

            genlist.Add(new AddNPCs());

            genlist.Add(new AddMapMods());

            genlist.Add(new SetupTerrainPatches());

            genlist.Add(new AddBridges());

            genlist.Add(new SmoothRoadEdges());

            genlist.Add(new SmoothHeightsFinal());

            genlist.Add(new AddWater());

            genlist.Add(new SetfinalTerrainHeights());

            genlist.Add(new AddTrees());

            genlist.Add(new AddBushes());

            genlist.Add(new AddRocks());

            genlist.Add(new AddFences());

            genlist.Add(new AddResourceNodes());

            genlist.Add(new AddClutter());

            genlist.Add(new AddChests());

            genlist.Add(new AddPlants());

            genlist.Add(new AddRoadBorders());

            genlist.Add(new DirtyRoads());

            genlist.Add(new AddSteepnessTextures());

            genlist.Add(new AddMountainTextures());

            genlist.Add(new AddRandomDirt());

            genlist.Add(new SetTerrainTextures());

            genlist.Add(new SetBelowLandTerrainTextures());

            genlist.Add(new SmoothTerrainTexturesFinal());

            genlist.Add(new SetFinalTerrainTextures());

            genlist.Add(new CreateMinimap());

            genlist.Add(new CreatePathfindingData());

            genlist.Add(new AddMonsterSpawns());

            genlist.Add(new AddQuests());

            genlist.Add(new SetMapSpawnPoint());

            genlist.Add(new SaveMap());

            genlist.Add(new UploadMap());

            genlist.Add(new AfterGenerateMap());

        }
        else
        {
            _assetService.SetLoadSpeed(ELoadSpeed.Fast);
            genlist.Add(new ClearMapData());

            genlist.Add(new SetupMapData());

            genlist.Add(new AddMinGroundLevel());

            genlist.Add(new LoadMinimap());

            genlist.Add(new LoadPathfinding());

            genlist.Add(new SetFinalRenderSettings());

            genlist.Add(new AfterGenerateMap());

            genlist.Add(new AddPlayerToMap());

            genlist.Add(new LoadInitialData());

        }

        foreach (IZoneGenerator gen in genlist)
        {
            _gs.loc.Resolve(gen);
        }

        StringBuilder output = new StringBuilder();
        DateTime totalStartTime = DateTime.UtcNow;

        int currStep = 0;
        int totalSteps = genlist.Count;
        while (genlist.Count > 0)
        {
            currStep++;
            IZoneGenerator gen = genlist[0];
            genlist.RemoveAt(0);
            ShowLoadingPercentEvent showPercent = new ShowLoadingPercentEvent()
            {
                CurrStep = currStep,
                TotalSteps = totalSteps,
            };
            _dispatcher.Dispatch(showPercent);
            DateTime startTime = DateTime.UtcNow;
            _logService.Debug("StageStart: " + currStep + " " + gen.GetType().Name + " Time: " + DateTime.UtcNow);
            try
            {
                await gen.Generate(token);
                _logService.Debug("StageEnd: " + currStep + " " + gen.GetType().Name + " Time: " + DateTime.UtcNow);
            }
            catch (Exception e)
            {
                ShowGenError(e.Message + "\n-----------\n" + e.StackTrace);
                return;
            }
            DateTime endTime = DateTime.UtcNow;

            output.Append("Stage: " + currStep + ": " + gen.GetType().Name + " -- " + (endTime - startTime).TotalSeconds + "\n");

            gen = null;


            await Awaitable.NextFrameAsync(cancellationToken: token);

            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        await Awaitable.NextFrameAsync(cancellationToken: token);

        await Awaitable.NextFrameAsync(cancellationToken: token);
        output.Append("------------------\n" +
            (DateTime.UtcNow - totalStartTime).TotalSeconds);

        _logService.Debug(output.ToString());

        // Wait for everything to get downloaded.

        _md.ClearGenerationData();

        await Awaitable.NextFrameAsync(cancellationToken: token);

        await Awaitable.NextFrameAsync(cancellationToken: token);

        _dispatcher.Dispatch(new MapIsLoadedEvent());
        _dispatcher.Dispatch(new OnNewGameData());
        _md.GeneratingMap = false;
        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
        _playerManager.MoveAboveObstacles();
        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);


        await Awaitable.NextFrameAsync(cancellationToken: token);


        _assetService.SetLoadSpeed(ELoadSpeed.Normal);
    }

    public override void ShowGenError(string msg)
    {
        base.ShowGenError(msg);
        _md.GeneratingMap = false;
    }




    public override void SetAllAlphamaps(float[,,] alphaMaps, CancellationToken token)
    {
        if (alphaMaps == null)
        {
            return;
        }
        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
            {
                float alphaTotal = 0.0f;
                for (int i = 0; i < TerrainTexChannels.Max; i++)
                {
                    _md.Alphas[x, z, i] = MathUtil.Clamp(0, _md.Alphas[x, z, i], 1);
                    alphaTotal += _md.Alphas[x, z, i];
                }
                if (alphaTotal <= 0)
                {
                    _md.Alphas[x, z, TerrainTexChannels.Base] = 0.75f;
                    _md.Alphas[x, z, TerrainTexChannels.Dirt] = 0.25f;
                }
                else
                {
                    for (int i = 0; i < TerrainTexChannels.Max; i++)
                    {
                        _md.Alphas[x, z, i] /= alphaTotal;
                        _md.Alphas[x, z, i] = MathUtil.Clamp(0, _md.Alphas[x, z, i], 1);
                    }
                }
            }
        }


        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gz = 0; gz < _mapProvider.GetMap().BlockCount; gz++)
            {
                TerrainData tdata = _terrainManager.GetTerrainData(gx, gz);
                if (tdata == null)
                {
                    continue;
                }

                TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gz);

                if (patch == null)
                {
                    continue;
                }

                if (patch.baseAlphas == null)
                {
                    patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, TerrainTexChannels.Max];
                }

                int startx = gz * (MapConstants.TerrainPatchSize - 1);
                int starty = gx * (MapConstants.TerrainPatchSize - 1);

                for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
                {
                    for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
                    {
                        patch.mainZoneIds[x, z] = (byte)_md.MapZoneIds[startx + x, starty + z];
                        patch.subZoneIds[x, z] = (byte)_md.SubZoneIds[startx + x, starty + z];
                        for (int index = 0; index < TerrainTexChannels.Max; index++)
                        {
                            patch.baseAlphas[x, z, index] = _md.Alphas[x + startx, z + starty, index];
                        }
                    }
                }


                _awaitableService.ForgetAwaitable(SetOnePatchAlphamaps(patch, token));
            }
        }

    }




    public override async Awaitable SetOnePatchAlphamaps(TerrainPatchData patch, CancellationToken token)
    {

        try
        {
            patch.HaveSetAlphamaps = false;
            Terrain terr = patch.Core.Terrain;
            TerrainData terrainData = patch.Core.TerrainData as TerrainData;

            int zoneIdCount = patch.FullZoneIdList.Count;


            Map map = _mapProvider.GetMap();

            List<long> zoneTypeIdList = new List<long>();

            foreach (long currZoneId in patch.FullZoneIdList)
            {
                zoneTypeIdList.Add(map.Get<Zone>(currZoneId).ZoneTypeId);
            }

            await _textureManager.SetupTerrainContainerLayers(patch, zoneTypeIdList, new List<long>(), token, true);

            int channelCount = patch.Core.Layers.Count;

            int size = MapConstants.TerrainPatchSize * MapConstants.AlphaMapsPerTerrainCell;
            float[,,] newAlphas = new float[size, size, channelCount];

            int pauseSize = MapConstants.TerrainPatchSize / 4;
            int pauseVal = pauseSize / 2;

            MyRandom rand = new MyRandom(patch.Core.GX * 13 + patch.Core.GZ * 17 + _mapProvider.GetMap().Seed / 3);

            if (patch.baseAlphas == null)
            {
                patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, TerrainTexChannels.Max];
            }

            int firstZoneIndex = -1;
            int zoneId = -1;
            int zoneIndex = -1;
            int otherZoneCheckLength = 13;
            float[] oneCellAlphas = new float[channelCount];
            float[] cellZoneWeights = new float[zoneIdCount];
            float tempAlphaTotal = 0;
            for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
            {
                if (x % pauseSize == pauseVal)
                {
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
                if (patch == null || patch.FullZoneIdList == null || patch.mainZoneIds == null)
                {
                    continue;
                }

                for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
                {
                    for (int c = 0; c < oneCellAlphas.Length; c++)
                    {
                        oneCellAlphas[c] = 0;
                    }
                    zoneIndex = -1;
                    zoneId = patch.mainZoneIds[x, z];
                    tempAlphaTotal = 0;
                    for (int i = 0; i < zoneIdCount; i++)
                    {
                        cellZoneWeights[i] = 0;
                    }

                    for (int i = 0; i < zoneIdCount; i++)
                    {
                        if (patch.FullZoneIdList[i] == zoneId)
                        {
                            firstZoneIndex = i;
                            cellZoneWeights[i] = 1;
                            zoneIndex = i;
                            break;
                        }
                    }
                    if (zoneIndex < 0)
                    {
                        zoneIndex = 0;
                        cellZoneWeights[0] = 1;
                    }

                    int baseZoneId = patch.subZoneIds[x, z];
                    bool adjacentToOtherZoneId = false;
                    if (baseZoneId > 0)
                    {
                        for (int xx = x - 1; xx <= x + 1; xx++)
                        {
                            if (xx < 0 || xx >= MapConstants.TerrainPatchSize)
                            {
                                continue;
                            }
                            for (int zz = z - 1; zz <= z + 1; zz++)
                            {
                                if (zz < 0 || zz >= MapConstants.TerrainPatchSize)
                                {
                                    continue;
                                }
                                if (patch.subZoneIds[xx, zz] != baseZoneId)
                                {
                                    adjacentToOtherZoneId = true;
                                    break;
                                }
                            }
                            if (adjacentToOtherZoneId)
                            {
                                break;
                            }
                        }
                    }

                    if (baseZoneId > 0)
                    {
                        if (patch.FullZoneIdList.Contains(baseZoneId))
                        {
                            float basePct = (adjacentToOtherZoneId ? 0.5f : 1);
                            for (int i = 0; i < cellZoneWeights.Length; i++)
                            {
                                if (patch.FullZoneIdList[i] == baseZoneId)
                                {
                                    cellZoneWeights[i] = basePct;
                                }
                                else
                                {
                                    cellZoneWeights[i] *= (1 - basePct);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int times = 0; times < 3; times++)
                        {
                            int offsetX = x + rand.Next() % (2 * otherZoneCheckLength + 1) - otherZoneCheckLength;
                            int offsetZ = z + rand.Next() % (2 * otherZoneCheckLength + 1) - otherZoneCheckLength;
                            if (offsetX >= 0 && offsetX < MapConstants.TerrainPatchSize &&
                                offsetZ >= 0 && offsetZ < MapConstants.TerrainPatchSize)
                            {
                                int offsetZoneIndex = -1;
                                for (int i = 0; i < zoneIdCount; i++)
                                {
                                    if (patch.FullZoneIdList[i] == patch.mainZoneIds[offsetX, offsetZ])
                                    {
                                        offsetZoneIndex = i;
                                        break;
                                    }
                                }
                                if (offsetZoneIndex > 0)
                                {
                                    for (int i = 0; i < zoneIdCount; i++)
                                    {
                                        cellZoneWeights[i] /= 2;
                                        if (i == offsetZoneIndex)
                                        {
                                            cellZoneWeights[i] += 0.5f;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int zid = 0; zid < zoneIdCount; zid++)
                    {
                        if (cellZoneWeights[zid] > 0)
                        {
                            Zone zone = _mapProvider.GetMap().Get<Zone>(patch.FullZoneIdList[zid]);

                            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(null).Get(zone.ZoneTypeId);

                            for (int index = 0; index < TerrainTexChannels.Max; index++)
                            {
                                long textureTypeId = zoneType.GetTerrainTextureIdFromChannel(index);

                                IndexedTerrainLayer indexedLayer = patch.Core.Layers.FastFirstOrDefault(x => x.TextureTypeId == textureTypeId);
                                if (indexedLayer != null)
                                {
                                    oneCellAlphas[indexedLayer.Index] = patch.baseAlphas[x, z, index] * cellZoneWeights[zid];
                                    tempAlphaTotal += oneCellAlphas[indexedLayer.Index];
                                }
                                else
                                {
                                    _logService.Info("Cell: " + patch.Core.GX + "-" + patch.Core.GZ + " Missing texture:" + textureTypeId + " in index " + index + " for " + zoneType.Name);
                                }
                            }
                        }
                    }

                    if (tempAlphaTotal < 0.01f)
                    {
                        oneCellAlphas[0] = 1;
                        await Awaitable.NextFrameAsync(cancellationToken: token);
                    }
                    else
                    {
                        for (int c = 0; c < channelCount; c++)
                        {
                            oneCellAlphas[c] /= tempAlphaTotal;
                        }
                    }

                    for (int xx = 0; xx < MapConstants.AlphaMapsPerTerrainCell; xx++)
                    {
                        for (int zz = 0; zz < MapConstants.AlphaMapsPerTerrainCell; zz++)
                        {
                            int xpos = x * MapConstants.AlphaMapsPerTerrainCell + xx;
                            int zpos = z * MapConstants.AlphaMapsPerTerrainCell + zz;

                            for (int i = 0; i < channelCount; i++)
                            {
                                newAlphas[xpos, zpos, i] = oneCellAlphas[i];
                            }
                        }
                    }
                }
            }

            if (terr == null || terr.terrainData == null)
            {
                return;
            }


            if (terr.terrainData.terrainLayers == null ||
                  terr.terrainData.terrainLayers.Length != newAlphas.GetLength(2))
            {
                _logService.Info("Setting wrong terrainLayer sizes on " + patch.Core.GX + " -- " + patch.Core.GZ);
            }

            if (terr == null || terr.terrainData == null)
            {
                return;
            }

            terr.terrainData.SetAlphamaps(0, 0, newAlphas);
            terr.Flush();
            _terrainManager.SetOneTerrainNeighbors(patch.Core.GX, patch.Core.GZ);
            patch.HaveSetAlphamaps = true;
        }
        catch (Exception ee)
        {
            _logService.Exception(ee, "SetOnePatchAlphamaps");
        }
    }


    public override void SetAllHeightmaps(float[,] heights, CancellationToken token)
    {
        if (heights == null)
        {
            return;
        }

        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gz = 0; gz < _mapProvider.GetMap().BlockCount; gz++)
            {
                SetOnePatchHeightmaps(_terrainManager.GetTerrainPatch(gx, gz), heights);
            }
        }

    }

    public override void SetOnePatchHeightmaps(TerrainPatchData patch, float[,] globalHeights, float[,] heightOverrides = null)
    {
        if (_gs == null || patch == null)
        {
            return;
        }

        Terrain terr = patch.Core.Terrain;
        TerrainData terrainData = patch.Core.TerrainData as TerrainData;

        if (terr == null || terrainData == null)
        {
            return;
        }


        int gx = patch.Core.GX;
        int gz = patch.Core.GZ;
        if (gx < 0 || gz < 0 || _md == null || gx >= _mapProvider.GetMap().BlockCount ||
            gz >= _mapProvider.GetMap().BlockCount)
        {
            return;
        }
        if (heightOverrides == null || heightOverrides.GetLength(0) < MapConstants.TerrainPatchSize ||
            heightOverrides.GetLength(1) < MapConstants.TerrainPatchSize)
        {
            heightOverrides = null;
            if (globalHeights == null)
            {
                return;
            }
        }

        if (heightOverrides != null)
        {
            terrainData.SetHeights(0, 0, heightOverrides);
            return;
        }

        float[,] localHeights = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        int startx = gz * (MapConstants.TerrainPatchSize - 1);
        int starty = gx * (MapConstants.TerrainPatchSize - 1);

        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int z = 0; z < MapConstants.TerrainPatchSize; z++)
            {
                if (heightOverrides != null)
                {
                    localHeights[x, z] = heightOverrides[x, z];
                }
                else
                {
                    localHeights[x, z] = globalHeights[startx + x, starty + z];
                }
            }
        }
        terrainData.SetHeights(0, 0, localHeights);

    }

    DateTime lastLoadClick = DateTime.UtcNow.AddMinutes(-1);
    public override void LoadMap(LoadIntoMapRequest loadData)
    {


#if UNITY_EDITOR
        if (string.IsNullOrEmpty(loadData.MapId) && !string.IsNullOrEmpty(InitClient.EditorInstance.CurrMapId))
        {
            loadData.MapId = InitClient.EditorInstance.CurrMapId;
        }
#else
        loadData.GenerateMap = false;  
#endif

        if (string.IsNullOrEmpty(loadData.MapId))
        {
            _logService.Info("No world id chosen!");
            return;
        }

        if ((DateTime.UtcNow - lastLoadClick).TotalSeconds < 5)
        {
            return;
        }
        lastLoadClick = DateTime.UtcNow;


        if (loadData.GenerateMap)
        {
            _logService.Info("Generating map " + loadData.MapId);
            LoadedMapId = "";
        }
        else
        {
            LoadedMapId = loadData.MapId;
            _logService.Info("Loading map " + loadData.MapId);
        }

        string postData = _serializer.SerializeToString(loadData);

        _webNetworkService.SendWebRequest(loadData, _gameToken);
    }

    public override async Awaitable OnLoadIntoMap(LoadIntoMapResponse data, CancellationToken token)
    {

        try
        {
            _gs.ch = new Character(data.Char);
            _gs.ch.ClientVersion = new Version(_appService.Version);
            _gs.ch.ClientPlatform = _appService.RuntimePlatform;
            CharacterUtils.CopyDataFromTo(data.Char, _gs.ch);
            _assetService.SetWorldAssetEnv(data.WorldDataEnv);
            _networkService.SetRealtimeEndpoint(data.Host, data.Port, data.Serializer);
            _dispatcher.Dispatch(new CloseAllScreens());
            _terrainManager.ClearPatches();
            _terrainManager.ClearMapObjects();

            _minimapService.SetTexture(null);

            if (data == null || data.Map == null || data.Char == null)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
                return;
            }

            _gs.ch.MapId = data.Map.Id;

            if (!data.Generating && (data.Map.Zones == null || data.Map.Zones.Count < 1))
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
                return;
            }

            foreach (IUnitData dataSet in data.CharData)
            {
                _gs.ch.Set(dataSet);
            }

            _gs.ch.Set(data.Stores);

            _mapProvider.SetMap(data.Map);

            _mapProvider.SetSpawns(new MapSpawnData() { Id = _mapProvider.GetMap().Id.ToString() });

            bool fixSeeds = false;

#if UNITY_EDITOR

            InitClient initComp = InitClient.EditorInstance;
            if (string.IsNullOrEmpty(LoadedMapId))
            {
                if (initComp.BlockCount >= 3)
                {
                    _mapProvider.GetMap().BlockCount = initComp.BlockCount;
                }
                if (initComp.ZoneSize >= 1)
                {
                    _mapProvider.GetMap().ZoneSize = initComp.ZoneSize;
                }
                if (initComp.MapGenSeed > 0)
                {
                    _mapProvider.GetMap().Seed = initComp.MapGenSeed;
                    fixSeeds = true;
                }
            }
#endif

            if (string.IsNullOrEmpty(LoadedMapId) && !fixSeeds)
            {
                _mapProvider.GetMap().Seed = (int)(DateTime.UtcNow.Ticks % 2000000000);
                foreach (Zone item in _mapProvider.GetMap().Zones)
                {
                    item.Seed = (int)(DateTime.UtcNow.Ticks % 1000000000 + item.IdKey * 235622);
                }
            }

            MapGenData mgd = new MapGenData();

            _gs.loc.Set<IMapGenData>(mgd);

            if (_gs.ch == null)
            {
                _gs.ch = new Character(new CoreCharacter());
            }
            _gs.loc.ResolveSelf();

            _terrainManager.ClearPatches();

            if (_mapProvider.GetMap() == null)
            {
                _dispatcher.Dispatch(new CloseAllScreens());
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.CharacterSelect));
                _logService.Info("Map failed to download");
                return;
            }

            InstantiateMap(_mapProvider.GetMap().Id);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "OnLoadIntoMap");
        }

        await Task.CompletedTask;
    }
}


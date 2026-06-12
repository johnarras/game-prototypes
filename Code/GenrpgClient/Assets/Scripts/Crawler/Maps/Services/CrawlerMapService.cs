using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Audio.ClientEvents;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Buildings;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Controllers;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.Maps;
using Assets.Scripts.Crawler.Maps.EncounterHelpers;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Props;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.Shared.GameEvents;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Dungeons;
using Assets.Scripts.Dungeons.Audio;
using Assets.Scripts.Dungeons.Audio.Constants;
using Assets.Scripts.GameObjects;
using Assets.Scripts.ProcGen.Materials;
using Assets.Scripts.UI.Crawler.CrawlerPanels;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Buildings.Settings;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.UI.Constants;
using OxDb.SharedGame.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{
    public interface ICrawlerMapService : IInitializable, IClientResetCleanup
    {
        Awaitable EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token);
        void MovePartyTo(PartyData party, int x, int z, int rot, bool showMinimap, CancellationToken token);
        void CleanMap();
        void MarkCurrentCellVisited();
        bool MarkCellVisitedAndCheckForCompletion(long mapId, int x, int z);
        bool PartyHasVisited(long mapId, int x, int z, bool thisRunOnly = false);
        int GetBlockingBits(CrawlerMap map, int sx, int sz, int ex, int ez, bool allowBuildingEntry);
        FullWallTileImage GetMinimapWallFilename(CrawlerMap map, int x, int z);
        bool InDungeonMap();
        bool InIndoorMap();
        ICrawlerMapTypeHelper GetMapHelper(long mapType);
        IClientMapEncounterHelper GetEncounterHelper(long encounterTypeId);
        void MarkCellCleansed(int x, int z);
        void UpdateCameraPos(CancellationToken token);
        CrawlerMapRoot GetMapRoot();
        int GetMagicBits(long mapId, int x, int z, bool modifyWithPartyBuffs);
        bool HasMagicBit(int x, int z, long bit, bool modifyWithPartyBuffs);
        string GetMapName(PartyData party, long mapId, int x, int z);
        int GetMapCellHash(long mapId, int x, int z, long extraData);
        long GetCurrentEncounterAtCell(PartyData party, CrawlerMap map, int x, int z, bool onlyIfCanTriggerNow);
        void ClearCellObject(int x, int z);
        void SetMapComplete(PartyData party, CrawlerWorld world, long mapId);
        EntranceMapData GetEntranceMap(PartyData party, CrawlerWorld world, long mapId);
        void PlayMapSounds();
        void LoadProp(CrawlerObjectLoadData loadData, string prefabName, CancellationToken token);

    }

    public class CrawlerMapService : ICrawlerMapService
    {
        private IAssetService _assetService = null;
        private ICameraController _cameraController = null;
        private ICrawlerService _crawlerService = null;
        private IDispatcher _dispatcher = null;
        private IGameData _gameData = null;
        private ICrawlerWorldService _worldService = null;
        private IClientEntityService _clientEntityService = null;
        private IAwaitableService _awaitableService = null;
        private IClientGameState _gs = null;
        private IScreenService _screenService = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerMoveService _moveService = null;
        private ICrawlerDrawMapService _drawMapService = null;
        private ICrawlerQuestService _questService = null;
        private IPartyService _partyService = null;
        private ICrawlerOptionsService _optionsService = null;
        private ICrawlerMapGenService _mapGenService = null;
        private ICrawlerTerrainService _terrainService = null;
        private IMaterialGenService _materialGenService = null;
        private ILogService _logService = null;

        public const string MaterialGenDataFilenameSuffix = "MaterialGenData";

        public const string DungeonAssetBlockListFilename = "DungeonAssetBlockList";

        CrawlerMapRoot _crawlerMapRoot = null;
        private CancellationToken _token;

        private GameObject _cameraParent = null;
        private Camera _camera = null;

        private PartyData _party;
        private CrawlerWorld _world;

        private FullWallTileImage[] TileImages { get; set; }

        private List<WallTileImage> _refImages { get; set; } = new List<WallTileImage>();

        private SetupDictionaryContainer<long, ICrawlerMapTypeHelper> _mapTypeHelpers = new SetupDictionaryContainer<long, ICrawlerMapTypeHelper>();

        private SetupDictionaryContainer<long, IClientMapEncounterHelper> _encounterHelpers = new SetupDictionaryContainer<long, IClientMapEncounterHelper> { };

        public CrawlerMapRoot GetMapRoot()
        {
            return _crawlerMapRoot;
        }

        private GameObject _playerLightObject = null;
        private Light _playerLight = null;
        private PlayerLightController _lightController = null;
        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            CreateWallImageGrid();
            await Task.CompletedTask;
        }

        public ICrawlerMapTypeHelper GetMapHelper(long mapType)
        {
            if (_mapTypeHelpers.TryGetValue(mapType, out ICrawlerMapTypeHelper helper))
            {
                return helper;
            }
            return null;
        }

        public IClientMapEncounterHelper GetEncounterHelper(long encounterTypeId)
        {
            if (_encounterHelpers.TryGetValue(encounterTypeId, out IClientMapEncounterHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Awaitable EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token)
        {

            if (_crawlerMapRoot != null)
            {
                _dispatcher.Dispatch(new PlaySound(CrawlerAudio.ClimbStairs));
            }
            _dispatcher.Dispatch(new SetAmbientSoundCategory(AmbientSoundCategoryNames.Loading));
            _dispatcher.Dispatch(new OpenScreen(ScreenNames.Loading));

            while (_screenService.GetScreen(ScreenNames.Loading) == null)
            {
                await Awaitable.NextFrameAsync(token);
            }

            await _assetService.UnloadUnusedAssetsAsync();

            _party = party;
            _world = await _worldService.GetWorld(_party.WorldId);

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                await OnEnterNewRoguelikeMap(party, _world, mapData.Map, token);
            }

            CleanMap();
            await Awaitable.NextFrameAsync(token);

            await _moveService.OnEnterMap(party, mapData, token);


            ICrawlerMapTypeHelper helper = GetMapHelper(mapData.Map.CrawlerMapTypeId);

            _crawlerMapRoot = await helper.EnterMap(party, mapData, token);

            _crawlerMapRoot.MapTypeHelper = helper;

            await LoadMapAssets(_world, party, _crawlerMapRoot, token);

            PlayMapSounds();

            MovePartyTo(party, _party.CurrPos.X, _party.CurrPos.Z, _party.CurrPos.Rot, true, token);

            _dispatcher.Dispatch(new UpdateCrawlerUI());

            if (_party.InitialCombat != null)
            {
                _crawlerService.ChangeState(ECrawlerStates.StartCombat, token);
            }
            else
            {
                await _crawlerService.SaveGame();
            }

            while (_assetService.IsDownloading())
            {
                await Awaitable.NextFrameAsync(token);
            }


            await SetupCameraAndLighting(token);

            _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));

            UpdateCameraPos(token);
        }

        public void PlayMapSounds()
        {
            PartyData party = _crawlerService.GetParty();

            if (_crawlerMapRoot != null && _crawlerMapRoot.Map != null)
            {
                CrawlerMapType mapType = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(_crawlerMapRoot.Map.CrawlerMapTypeId);

                if (mapType != null)
                {
                    _dispatcher.Dispatch(new SetAmbientSoundCategory(mapType.Name));
                }
                else
                {
                    _dispatcher.Dispatch(new SetAmbientSoundCategory(null));
                }
            }
        }

        private async Awaitable SetupCameraAndLighting(CancellationToken token)
        {
            if (_playerLight == null)
            {
                _cameraParent = _cameraController?.GetCameraParent();
                _camera = _cameraController.GetMainCamera();
                _camera.transform.position = Vector3.zero;
                _camera.transform.localPosition = new Vector3(0, 0, -_crawlerMapRoot.XZBlockSize * 0.4f);
                _camera.transform.eulerAngles = new Vector3(0, 0, 0);
                _camera.farClipPlane = _crawlerMapRoot.XZBlockSize * CrawlerDrawMapService.ViewRadius;
                _camera.fieldOfView = 75f;

                _playerLightObject = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.UI, "PlayerLight", _cameraParent, _token, "Units"));
                _playerLight = _clientEntityService.GetComponent<Light>(_playerLightObject);
                _playerLight.intensity = 100;
                _playerLight.range = 80;
                _playerLight.color = new UnityEngine.Color(1.0f, 0.95f, 0.9f, 1.0f);
                _lightController = _playerLightObject.GetComponent<PlayerLightController>();
                _lightController.Init();
                await Awaitable.NextFrameAsync(token);
                _playerLight.transform.position = Vector3.zero;
                _playerLightObject.transform.position = Vector3.zero;
                await Awaitable.NextFrameAsync(token);
            }
        }

        private async Task LoadMapAssets(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, CancellationToken token)
        {

            string assetBlockPrefix = "Basic";

            if (false && mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
            {
                assetBlockPrefix = "Wide";
            }

            _assetService.LoadAsset(AssetCategoryNames.Dungeons, assetBlockPrefix + DungeonAssetBlockListFilename, OnLoadDungeonAssetBlock, _crawlerMapRoot.AssetRoot, token, mapRoot);

            if (_crawlerMapRoot.Map.CrawlerMapTypeId != CrawlerMapTypes.Dungeon)
            {
                string buildingArtFolder = _gameData.Get<BuildingArtSettings>(_gs.ch).Get(mapRoot.Map.BuildingArtId).Art;

                _assetService.LoadAsset(AssetCategoryNames.Buildings, "CityAssets", OnLoadCityAssets, _crawlerMapRoot.AssetRoot, token, default(object), buildingArtFolder);

            }

            while (!mapRoot.AssetsAreReady())
            {
                await Task.Delay(1);
            }

            await _terrainService.DrawTerrain(world, party, mapRoot, token);

        }
        private void OnLoadCityAssets(object obj, object data, CancellationToken token)
        {
            GameObject assetGo = obj as GameObject;

            if (assetGo == null)
            {
                return;
            }

            _crawlerMapRoot.CityAssets = assetGo.GetComponent<CityAssets>();

            _assetService.LoadAsset(AssetCategoryNames.Dungeons, "Building" + MaterialGenDataFilenameSuffix, OnLoadBuildingMaterialData,
                _crawlerMapRoot.AssetRoot, token, _crawlerMapRoot);

        }

        private void OnLoadBuildingMaterialData(GameObject go, CrawlerMapRoot mapRoot, CancellationToken token)
        {

            _awaitableService.ForgetAwaitable(OnLoadBuildingMaterialDataAsync(go, mapRoot, token));
        }

        private async Awaitable OnLoadBuildingMaterialDataAsync(GameObject go, CrawlerMapRoot mapRoot, CancellationToken token)
        {

            MaterialGenData materialsData = _clientEntityService.FullInstantiate(go.GetComponent<MaterialGenData>());
            _clientEntityService.AddToParent(materialsData, mapRoot.AssetRoot);

            WallTextureGenArgs args = new WallTextureGenArgs()
            {
                MaterialsData = materialsData,
                Seed = mapRoot.Map.ArtSeed + 292381,
                MapRoot = mapRoot,
            };

            Texture2D[] textures = await _materialGenService.GenerateMultipleLooseTexturesForOneMaterialIndex(args, DungeonMaterialIndexes.Stone, 2);

            foreach (Texture2D tex in textures)
            {
                Material mat = new Material(materialsData.MainMaterial);

                mat.mainTexture = tex;

                Texture2D normalTex = _materialGenService.CreateGrayscaleNormalMapFromDiffuseTexture(tex, false, 1);

                _materialGenService.SetNormalMap(mat, normalTex);

                MaterialOption weighted = new MaterialOption()
                {
                    Mat = mat,
                };

                mapRoot.BuildingWallOptions.Add(weighted);

                mapRoot.GeneratedTextures.Add(tex);

                mapRoot.GeneratedTextures.Add(normalTex);

            }

            foreach (WeightedCrawlerBuilding weightedBuilding in mapRoot.CityAssets.Buildings)
            {
                weightedBuilding.Mats.WallMats.Clear();

                foreach (MaterialOption opt in mapRoot.BuildingWallOptions)
                {
                    weightedBuilding.Mats.WallMats.Add(new WeightedBuildingMaterial()
                    {
                        Mat = opt.Mat,
                        Weight = 1000,
                        ColorTargets = new List<Color>(materialsData.ColorSets.Select(x => x.Foreground)),
                    });
                }

                foreach (WeightedBuildingMaterial weightedMat in weightedBuilding.Mats.WallMats)
                {
                    for (int c = 0; c < weightedMat.ColorTargets.Count; c++)
                    {
                        weightedMat.ColorTargets[c] = weightedMat.ColorTargets[c] * 0.75f;
                    }
                }
            }
        }

        private void OnLoadDungeonAssetBlock(GameObject go, CrawlerMapRoot mapRoot, CancellationToken token)
        {
            DungeonAssetBlockList list = go.GetComponent<DungeonAssetBlockList>();

            if (list == null || list.Blocks.Count < 1)
            {
                return;
            }

            mapRoot.AssetBlockList = list;

            IRandom rand = new MyRandom(mapRoot.Map.ArtSeed * 17);

            mapRoot.AssetBlock = RandUtils.GetRandomElement(list.Blocks, rand);
            mapRoot.AssetBlock = list.Blocks[1];
            mapRoot.XZBlockSize = mapRoot.AssetBlockList.BlockXZSize;
            mapRoot.YBlockSize = mapRoot.AssetBlockList.BlockYSize;
            mapRoot.PillarAsset = RandUtils.GetRandomElement(mapRoot.AssetBlock.Pillars, rand).Asset;


            if (rand.NextDouble() < mapRoot.AssetBlockList.VaultedCeilingChance &&
                mapRoot.AssetBlockList.VaultedCeilings != null)
            {
                List<VaultedCeilingAssetBlock> validAssets = mapRoot.AssetBlockList.VaultedCeilings.Where(v => v.IsValid()).ToList();

                if (validAssets.Count > 0)
                {
                    mapRoot.VaultedCeilingAssets = RandUtils.GetRandomElement(validAssets, rand);
                }
            }

            List<long> currentZoneTypes = mapRoot.GetAllTerrainZoneTypes();

            List<long> dungeonZoneTypes = new List<long>();

            CrawlerMapType mapType = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(CrawlerMapTypes.Dungeon);

            List<long> allDungeonZoneTypes = new List<long>();

            foreach (CrawlerMapGenType genType in mapType.GenTypes)
            {
                allDungeonZoneTypes.AddRange(genType.WeightedZones.Select(x => x.ZoneTypeId));
            }

            if (mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.City)
            {
                allDungeonZoneTypes.Add(mapRoot.Map.ZoneTypeId);
            }

            allDungeonZoneTypes = allDungeonZoneTypes.Distinct().ToList();

            foreach (long zoneTypeId in currentZoneTypes)
            {
                if (allDungeonZoneTypes.Contains(zoneTypeId))
                {
                    ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

                    if (ztype != null)
                    {
                        MaterialBlock block = new MaterialBlock() { ZoneTypeId = ztype.IdKey };

                        mapRoot.MaterialBlocks[zoneTypeId] = block;

                        string dungeonArtName = ztype.Art;

                        _assetService.LoadAsset(AssetCategoryNames.Dungeons, dungeonArtName + MaterialGenDataFilenameSuffix, OnLoadDungeonMaterialsData, _crawlerMapRoot.AssetRoot, token, block);
                    }
                }
            }
        }


        private void OnLoadDungeonMaterialsData(GameObject assetGo, MaterialBlock block, CancellationToken token)
        {

            if (block == null)
            {
                return;
            }

            _awaitableService.ForgetAwaitable(OnLoadDungeonMaterialsDataAsync(assetGo, block, token));
        }

        private async Awaitable OnLoadDungeonMaterialsDataAsync(GameObject assetGo, MaterialBlock block, CancellationToken token)
        {
            long materialSeed = _crawlerMapRoot.Map.ArtSeed / 5 + 1433 + block.ZoneTypeId;

            IRandom rand = new MyRandom(materialSeed);

            MaterialGenData materialsData = _clientEntityService.FullInstantiate(assetGo.GetComponent<MaterialGenData>());
            _clientEntityService.AddToParent(materialsData, _crawlerMapRoot.AssetRoot);

            WallTextureGenArgs genArgs = new WallTextureGenArgs()
            {
                Seed = _crawlerMapRoot.Map.ArtSeed + block.ZoneTypeId,
                MaterialsData = materialsData,
                ZoneTypeId = block.ZoneTypeId,
                MapRoot = _crawlerMapRoot,
            };

            GeneratedWallLooseTextureSet textureSet = await _materialGenService.GenerateTextures(genArgs);

            foreach (Texture2D tex in textureSet.DiffuseTextures)
            {
                if (tex != null)
                {
                    _crawlerMapRoot.GeneratedTextures.Add(tex);
                }
            }

            foreach (Texture2D normal in textureSet.NormalTextures)
            {
                if (normal != null)
                {
                    _crawlerMapRoot.GeneratedTextures.Add(normal);
                }
            }

            for (int materialIndex = 0; materialIndex < DungeonMaterialIndexes.Max; materialIndex++)
            {
                int materialCount = 1;

                for (int m = 0; m < materialCount; m++)
                {
                    Material mat = new Material(materialsData.MainMaterial);

                    MaterialOption weighted = new MaterialOption()
                    {
                        Mat = mat,
                    };

                    weighted.Mat.mainTexture = textureSet.DiffuseTextures[materialIndex];

                    _materialGenService.SetNormalMap(weighted.Mat, textureSet.NormalTextures[materialIndex]);

                    block.FinalMaterials.GetMaterials(materialIndex).Add(weighted);



                }
            }
        }

        public async Task OnReset(CancellationToken token)
        {
            CleanMap();
            await Task.CompletedTask;
        }


        public void CleanMap()
        {
            if (_crawlerMapRoot != null)
            {
                foreach (MaterialBlock block in _crawlerMapRoot.MaterialBlocks.Values)
                {
                    block.FinalMaterials = null;
                }
                _crawlerMapRoot.MaterialBlocks.Clear();
                if (_crawlerMapRoot.CityAssets != null)
                {
                    _clientEntityService.Destroy(_crawlerMapRoot.CityAssets.gameObject);
                    _crawlerMapRoot.CityAssets = null;
                }
                _clientEntityService.Destroy(_crawlerMapRoot.gameObject);

                if (_crawlerMapRoot.AssetBlockList != null)
                {
                    _crawlerMapRoot.AssetBlock = null;
                    _crawlerMapRoot.AssetBlockList.Clear();
                    _clientEntityService.Destroy(_crawlerMapRoot.AssetBlockList);
                    _crawlerMapRoot.AssetBlockList = null;
                }

            }
            _crawlerMapRoot = null;
        }

        public void UpdateCameraPos(CancellationToken token)
        {
            if (_crawlerMapRoot == null || _playerLight == null)
            {
                return;
            }

            if (!InDungeonMap())
            {
                _playerLight.intensity = 0;
            }
            _cameraParent.transform.position = new Vector3(_crawlerMapRoot.DrawX, _crawlerMapRoot.DrawY, _crawlerMapRoot.DrawZ);
            _cameraParent.transform.eulerAngles = new Vector3(0, _crawlerMapRoot.DrawRot + 90, 0);
            _dispatcher.Dispatch(new ShowWorldPanelImage(null));
            _playerLightObject.transform.position = _camera.transform.position;

        }


        public int GetBlockingBits(CrawlerMap map, int sx, int sz, int ex, int ez, bool allowBuildingEntry)
        {
            ICrawlerMapTypeHelper helper = GetMapHelper(map.CrawlerMapTypeId);

            return helper.GetBlockingBits(map, sx, sz, ex, ez, allowBuildingEntry);
        }


        public void MarkCurrentCellVisited()
        {
            if (_party == null || _party.Combat != null &&
                _crawlerMapRoot == null || _crawlerMapRoot.Map == null ||
                _party.CurrPos.MapId != _crawlerMapRoot.Map.IdKey ||
                _party.CurrPos.X < 0 || _party.CurrPos.Z < 0 ||
                _party.CurrPos.X >= _crawlerMapRoot.Map.Width ||
                _party.CurrPos.Z >= _crawlerMapRoot.Map.Height)
            {
                return;
            }

            MarkCellVisitedAndCheckForCompletion(_party.CurrPos.MapId, _party.CurrPos.X, _party.CurrPos.Z);
        }

        public void SetMapComplete(PartyData party, CrawlerWorld world, long mapId)
        {

            CrawlerMap map = world.GetMap(mapId);

            if (map == null)
            {
                return;
            }

            _party.CompletedMaps.SetBitIndex(map.IdKey);
            for (int xx = 0; xx < map.Width; xx++)
            {
                for (int zz = 0; zz < map.Height; zz++)
                {
                    long questItemId = map.GetEntityId(xx, zz, EntityTypes.QuestItem);
                    if (questItemId > 0)
                    {
                        _party.QuestItems.SetBitIndex(questItemId);
                    }
                }
            }

            _questService.GiveExploreQuestCredit(party, mapId);
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party, PartyArrowOnly = false });

        }

        public void MarkCellCleansed(int x, int z)
        {
            if (_party == null || _world == null)
            {
                return;
            }

            CrawlerMap map = _world.GetMap(_party.CurrPos.MapId);

            if (map == null)
            {
                return;
            }

            _party.CurrentMap.Cleansed.SetBitIndex(map.GetIndex(x, z));

            MapEncounterType encounterType = _gameData.Get<MapEncounterSettings>(_gs.ch).Get(GetCurrentEncounterAtCell(_party, map, x, z, true));

            if (encounterType != null && encounterType.CanBeCleansed)
            {
                ClearCellObject(x, z);
            }
        }
        public bool MarkCellVisitedAndCheckForCompletion(long mapId, int x, int z)
        {
            if (_party == null || _world == null)
            {
                return false;
            }

            CrawlerMap map = _world.GetMap(mapId);
            if (map == null)
            {
                return false;
            }

            int index = map.GetIndex(x, z);

            _party.CurrentMap.Visited.SetBitIndex(index);

            if (map.CrawlerMapTypeId == CrawlerMapTypes.City)
            {
                SetMapComplete(_party, _world, map.IdKey);
                return false;
            }

            if (_party.CompletedMaps.HasBitIndex(mapId))
            {
                return false;
            }

            CrawlerMapStatus status = _party.GetMapStatus(mapId, true);

            if (status.TotalCells < 1)
            {
                for (int mx = 0; mx < map.Width; mx++)
                {
                    for (int mz = 0; mz < map.Height; mz++)
                    {
                        if (map.Get(mx, mz, CellIndex.Terrain) > 0)
                        {
                            status.TotalCells++;
                        }
                    }
                }
            }

            if (!status.Visited.HasBitIndex(index))
            {
                status.CellsVisited++;
            }

            status.Visited.SetBitIndex(index);

            // On map complete, mark all previous maps as complete.
            if (status.CellsVisited >= status.TotalCells)
            {
                _party.AddFlags(PartyFlags.HasRecall);
                SetMapComplete(_party, _world, status.MapId);
                NewUpgradePointsResult result = _upgradeService.GetNewPartyUpgradePoints(_party, map.Level, UpgradeReasons.CompleteDungeon);

                foreach (string msg in result.Messages)
                {
                    _dispatcher.Dispatch(new AddActionPanelText(msg));
                }

                List<CrawlerMapStatus> partyMaps = new List<CrawlerMapStatus>(_party.Maps);

                List<CrawlerMap> allMaps = _world.Maps.ToList();

                // Once you complete a dungeon, mark all previous levels in that dungeon as complete.
                foreach (CrawlerMap cm in allMaps)
                {
                    if (cm.Level < map.Level && cm.CrawlerMapTypeId == CrawlerMapTypes.Dungeon &&
                        cm.BaseCrawlerMapId == map.BaseCrawlerMapId)
                    {
                        CrawlerMapStatus dungeonStatus = partyMaps.FirstOrDefault(x => x.MapId == cm.IdKey);

                        if (dungeonStatus != null)
                        {
                            _party.Maps.Remove(dungeonStatus);
                        }

                        if (!_party.CompletedMaps.HasBitIndex(cm.IdKey))
                        {
                            SetMapComplete(_party, _world, cm.IdKey);

                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool PartyHasVisited(long mapId, int x, int z, bool thisRunOnly = false)
        {
            if (_party == null || _world == null)
            {
                return false;
            }

            CrawlerMap map = _world.GetMap(mapId);
            if (map == null)
            {
                return false;
            }
            x = MathUtil.ModClamp(x, map.Width);
            z = MathUtil.ModClamp(z, map.Height);

            if (x < 0 || x >= map.Width || z < 0 || z >= map.Height)
            {
                return false;
            }

            if (thisRunOnly)
            {
                return _party.CurrentMap.Visited.HasBitIndex(map.GetIndex(x, z));
            }


            if (_party.CompletedMaps.HasBitIndex(mapId))
            {
                return true;
            }


            CrawlerMapStatus status = _party.GetMapStatus(mapId, false);
            if (status == null)
            {
                return false;
            }

            int index = map.GetIndex(x, z);

            return status.Visited.HasBitIndex(index);
        }

        public void MovePartyTo(PartyData party, int x, int z, int rot, bool showMinimap, CancellationToken token)
        {
            if (_crawlerMapRoot == null)
            {
                return;
            }

            x = MathUtil.Clamp(0, x, _crawlerMapRoot.Map.Width - 1);
            z = MathUtil.Clamp(0, z, _crawlerMapRoot.Map.Height - 1);

            _crawlerMapRoot.DrawX = x * _crawlerMapRoot.XZBlockSize;
            _crawlerMapRoot.DrawZ = z * _crawlerMapRoot.XZBlockSize;
            party.CurrPos.X = x;
            party.CurrPos.Z = z;
            party.CurrPos.Rot = rot;
            UpdateCameraPos(token);
            MarkCurrentCellVisited();
            _awaitableService.ForgetAwaitable(_drawMapService.DrawNearbyMap(_party, _world, _crawlerMapRoot, token));

            if (showMinimap)
            {
                _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party, PartyArrowOnly = false });
            }

            _dispatcher.Dispatch(new MovePartyEvent());
        }

        private int IgnoreSecret(int wallVal)
        {
            if (wallVal == WallTypes.Barricade)
            {
                return WallTypes.None;
            }
            else if (wallVal == WallTypes.Secret)
            {
                return WallTypes.Wall;
            }
            return wallVal;
        }
        public FullWallTileImage GetMinimapWallFilename(CrawlerMap map, int x, int z)
        {
            StringBuilder sb = new StringBuilder();

            int index = 0;

            index += IgnoreSecret(map.NorthWall(x, (z + map.Height - 1) % map.Height));
            index *= 3;
            index += IgnoreSecret(map.EastWall((x + map.Width - 1) % map.Width, z));
            index *= 3;
            index += IgnoreSecret(map.NorthWall(x, z));
            index *= 3;
            index += IgnoreSecret(map.EastWall(x, z));

            FullWallTileImage img = TileImages[index];

            return img;
        }

        private bool _didInitWallImages = false;
        string _wallLetterList = "OWDW";
        private void CreateWallImageGrid()
        {
            if (_didInitWallImages)
            {
                return;
            }
            _didInitWallImages = true;
            TileImages = new FullWallTileImage[TileImageConstants.ArraySize];

            for (int i = 0; i < TileImageConstants.ArraySize; i++)
            {
                int div = 1;

                int[] vals = new int[TileImageConstants.ArraySize];
                for (int w = 0; w < TileImageConstants.WallCount; w++)
                {
                    vals[w] = (i / div) % 3;
                    div *= 3;
                }

                bool didFindRefImage = false;
                for (int k = 0; k < _refImages.Count; k++)
                {
                    WallTileImage wti = _refImages[k];

                    for (int rot = 0; rot < TileImageConstants.WallCount; rot++)
                    {
                        bool anyWrong = false;

                        for (int w = 0; w < TileImageConstants.WallCount; w++)
                        {
                            if (wti.WallIds[(rot + w) % 4] != vals[w])
                            {
                                anyWrong = true;
                                break;
                            }
                        }

                        if (anyWrong)
                        {
                            continue;
                        }

                        TileImages[i] = new FullWallTileImage() { Index = i, Filename = wti.Filename, RefImage = wti, RotAngle = ((4 - rot) % 4) * 90, };

                        didFindRefImage = true;
                        break;
                    }
                }

                if (!didFindRefImage)
                {
                    WallTileImage wti = new WallTileImage() { WallIds = vals };
                    _refImages.Add(wti);
                    StringBuilder sb = new StringBuilder();
                    for (int w = 0; w < TileImageConstants.WallCount; w++)
                    {
                        sb.Append(_wallLetterList[vals[w]]);
                    }

                    wti.Filename = sb.ToString() + SpriteNameCategories.Wall;
                    TileImages[i] = new FullWallTileImage() { Index = i, Filename = wti.Filename, RefImage = wti };
                }
            }

        }

        public bool InDungeonMap()
        {
            return _crawlerMapRoot != null && _crawlerMapRoot.Map != null && _crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon;
        }

        public bool InIndoorMap()
        {
            return _crawlerMapRoot != null && _crawlerMapRoot.Map != null && _crawlerMapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoors);
        }

        public bool HasMagicBit(int x, int z, long bit, bool modifyWithPartyBuffs)
        {
            return FlagUtils.MatchesAnyBits(GetMagicBits(_party.CurrPos.MapId, x, z, modifyWithPartyBuffs), (1 << (int)bit));
        }

        public int GetMagicBits(long mapId, int x, int z, bool modifyWithPartyBuffs)
        {
            if (_world == null)
            {
                return 0;
            }

            CrawlerMap map = _world.GetMap(mapId);

            if (map == null)
            {
                return 0;
            }

            int bits = map.GetEntityId(x, z, EntityTypes.MapMagic);

            if (mapId == _party.CurrPos.MapId && _party.CurrentMap.Cleansed.HasBitIndex(map.GetIndex(x, z)))
            {
                return 0;
            }

            if (modifyWithPartyBuffs)
            {
                IReadOnlyList<MapMagicType> magicList = _gameData.Get<MapMagicSettings>(_gs.ch).GetData();

                foreach (MapMagicType mtype in magicList)
                {
                    if (_partyService.HasPartyBuff(_party, EntityTypes.MapMagic, mtype.IdKey))
                    {
                        bits &= (1 << ((int)mtype.IdKey));
                    }
                }
            }

            return bits << 1;
        }

        public string GetMapName(PartyData party, long mapId, int x, int z)
        {
            if (_world == null)
            {
                return "";
            }
            CrawlerMap map = _world.GetMap(mapId);

            if (map == null)
            {
                return "The Unknown Regions";
            }
            long regionId = map.Get(x, z, CellIndex.Region);

            ZoneRegion region = map.Regions?.FirstOrDefault(x => x.ZoneTypeId == regionId);

            if (region != null)
            {
                return region.Name;
            }

            return map.Name;
        }

        public int GetMapCellHash(long mapId, int x, int z, long extraData)
        {
            return (int)(mapId * 13 + x * 23 + z * 41 + extraData * 59);
        }

        virtual public long GetCurrentEncounterAtCell(PartyData party, CrawlerMap map, int x, int z, bool onlyIfCanTriggerNow)
        {
            int encounterTypeId = map.GetEntityId(x, z, EntityTypes.MapEncounter);

            if (encounterTypeId < 1)
            {
                return 0;
            }

            MapEncounterType etype = _gameData.Get<MapEncounterSettings>(_gs.ch).Get(encounterTypeId);
            if (etype == null)
            {
                return 0;
            }

            if (etype.CanBeCleansed && party.CurrentMap.Cleansed.HasBitIndex(map.GetIndex(x, z)))
            {
                return 0;
            }


            if (!etype.CanRepeat)
            {
                // Check if map is completed or we have the one-time flag set.
                // If didn't visit now and didn't complete map, then the encounter is there.
                if (party.CompletedMaps.HasBitIndex(map.IdKey) && party.LastAutoCompleteLevel != map.IdKey)
                {
                    return 0;
                }

                // Map not completed, so did we ever finish this encounter?

                CrawlerMapStatus mapStatus = party.GetMapStatus(map.IdKey, false);

                // If no status yet, we can do it.
                if (mapStatus == null)
                {
                    return encounterTypeId;
                }

                // If we did this encounter, return false, otherwise return true.
                PointXZ pt = mapStatus.OneTimeEncounters.FirstOrDefault(o => o.X == x && o.Z == z);

                if (pt != null)
                {
                    return 0;
                }
            }

            else // Can repeat, just check if we've been here this run.
            {

                if (onlyIfCanTriggerNow)
                {
                    if (PartyHasVisited(map.IdKey, x, z, true))
                    {
                        return 0;
                    }
                }
                else
                {
                    if (!PartyHasVisited(map.IdKey, x, z, false))
                    {
                        return 0;
                    }
                }
            }

            // Return true last so we can add other conditions here later if needed.

            return encounterTypeId;
        }

        public void ClearCellObject(int x, int z)
        {
            for (int xx = -1; xx <= 1; xx++)
            {
                int cx = x + _crawlerMapRoot.Map.Width * xx;
                for (int zz = -1; zz <= 1; zz++)
                {
                    int cz = z + _crawlerMapRoot.Map.Height * zz;
                    ClientMapCell cell = _crawlerMapRoot.GetCellAtWorldPos(x, z, false);
                    if (cell != null)
                    {
                        foreach (GameObject go in cell.Props)
                        {
                            _clientEntityService.Destroy(go);
                        }
                        cell.Props.Clear();
                    }
                }
            }
        }

        private async Awaitable OnEnterNewRoguelikeMap(PartyData party, CrawlerWorld world, CrawlerMap currMap, CancellationToken token)
        {
            if (currMap.IdKey == party.CurrPos.MapId)
            {
                return;
            }

            long mapId = currMap.IdKey;

            CrawlerMap cityMap = _world.GetMap(1);
            CrawlerMap dungeonMap = _world.GetMap(2);

            party.CompletedMaps.Clear();
            party.CompletedMaps.SetBitIndex(1);
            party.Maps.Clear();

            if (currMap.IdKey == 1 && world.GetMap(2) == null)
            {
                MapCellDetail exitDetail = cityMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == 2);
                world.AddMap(await _mapGenService.GenerateRoguelikeDungeonLevel(party, world, 2, exitDetail.X, exitDetail.Z, token));
            }

            int mapCount = world.Maps.Count;

            world.Maps = world.Maps.Where(x => x.IdKey == 1 || x.IdKey == 2 || x.IdKey == currMap.IdKey).ToList();
            world.ClearCache();

            bool changedCityLevel = false;

            if (cityMap != null && cityMap.Level < party.MaxLevelEntered)
            {
                cityMap.Level = party.MaxLevelEntered;
                changedCityLevel = true;
                party.VendorItems.Clear();
                party.VendorBuyback.Clear();
                party.LastVendorRefresh = DateTime.UtcNow.AddDays(-1);
            }

            if (world.Maps.Count != mapCount || changedCityLevel)
            {
                await _worldService.SaveWorld(world);
            }
        }

        public EntranceMapData GetEntranceMap(PartyData party, CrawlerWorld world, long mapId)
        {
            EntranceMapData retval = new EntranceMapData();

            CrawlerMap targetMap = world.GetMap(mapId);

            if (targetMap == null || targetMap.CrawlerMapTypeId == CrawlerMapTypes.Outdoors)
            {
                return retval;
            }

            List<MapCellDetail> details = targetMap.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            List<CrawlerMap> entranceMaps = new List<CrawlerMap>();

            foreach (MapCellDetail detail in details)
            {
                CrawlerMap otherMap = world.GetMap(detail.EntityId);

                if (otherMap != null && otherMap.CrawlerMapTypeId != CrawlerMapTypes.Dungeon)
                {
                    entranceMaps.Add(otherMap);
                }
            }

            if (entranceMaps.Count > 0)
            {
                retval.EntranceMap = entranceMaps[0];

                MapCellDetail detail = retval.EntranceMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map &&
                x.EntityId == targetMap.IdKey);

                if (detail != null)
                {
                    retval.EntranceMapName = GetMapName(party, retval.EntranceMap.IdKey, detail.X, detail.Z);
                    retval.EnterX = detail.X;
                    retval.EnterZ = detail.Z;

                }
            }

            return retval;
        }

        public void LoadProp(CrawlerObjectLoadData loadData, string prefabName, CancellationToken token)
        {
            loadData.PrefabName = prefabName;
            _assetService.LoadAssetInto<CrawlerObjectLoadData>(loadData.Cell.gameObject,
                loadData.AssetCategoryNameOverride ?? AssetCategoryNames.Props, prefabName, OnDownloadProp, token, loadData);
        }

        private void OnDownloadProp(GameObject go, CrawlerObjectLoadData loadData, CancellationToken token)
        {
            if (go == null)
            {
                _logService.Error("Missing world object prefab " + loadData.PrefabName);
                return;
            }

            go.transform.eulerAngles = new Vector3(0, loadData.Angle, 0);

            loadData.Cell.Props.Add(go);

            go.name = go.name + "-" + loadData.Cell.MapX + "." + loadData.Cell.MapZ + "--" + go.transform.position / 8;
            CrawlerProp prop = _clientEntityService.GetComponent<CrawlerProp>(go);

            if (prop != null)
            {
                prop.SetData(loadData);
            }

        }
    }
}


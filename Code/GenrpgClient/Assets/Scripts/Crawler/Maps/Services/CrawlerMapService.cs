using Assets.Scripts.Assets;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Buildings;
using Assets.Scripts.Controllers;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Assets.Scripts.Crawler.Maps.EncounterHelpers;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Dungeons;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Settings;
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
        bool IsIndoors();
        ICrawlerMapTypeHelper GetMapHelper(long mapType);
        IClientMapEncounterHelper GetEncounterHelper(long encounterTypeId);
        string GetBGImageName();
        void MarkCellCleansed(int x, int z);
        void UpdateCameraPos(CancellationToken token);
        CrawlerMapRoot GetMapRoot();
        int GetMagicBits(long mapId, int x, int z, bool modifyWithPartyBuffs);
        bool HasMagicBit(int x, int z, long bit, bool modifyWithPartyBuffs);
        string GetMapName(PartyData party, long mapId, int x, int z);
        int GetMapCellHash(long mapId, int x, int z, long extraData);
        long GetEncounterAtCell(PartyData party, CrawlerMap map, int x, int z);
        void ClearCellObject(int x, int z);
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

        public string GetBGImageName()
        {
            if (_party == null)
            {
                _party = _crawlerService.GetParty();
            }

            if (_party.Combat != null)
            {
                return "Battlefield";
            }


            ZoneType zoneType = _worldService.GetCurrentZone(_party).Result;

            if (zoneType != null && !string.IsNullOrEmpty(zoneType.Icon))
            {
                return zoneType.Icon;
            }
            return CrawlerClientConstants.DefaultWorldBG;

        }

        private GameObject _playerLightObject = null;
        private Light _playerLight = null;
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
            _screenService.Open(ScreenNames.Loading);

            while (_screenService.GetScreen(ScreenNames.Loading) == null)
            {
                await Awaitable.NextFrameAsync(token);
            }

            CleanMap();
            await Awaitable.NextFrameAsync(token);
            _party = party;
            _world = await _worldService.GetWorld(_party.WorldId);
            await _moveService.EnterMap(party, mapData, token);

            if (_playerLight == null)
            {
                _cameraParent = _cameraController?.GetCameraParent();
                if (_playerLightObject == null)
                {
                    _playerLightObject = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.UI, "PlayerLight", _cameraParent, _token, "Units"));
                }
                _playerLight = _clientEntityService.GetComponent<Light>(_playerLightObject);

                if (_playerLight != null)
                {
                    _playerLight.color = new UnityEngine.Color(1.0f, 0.9f, 0.8f, 1.0f);
                    _playerLight.intensity = 0;
                }

                PlayerLightController plc = _playerLightObject.GetComponent<PlayerLightController>();
                if (plc != null)
                {
                    plc.enabled = false;
                    plc.Init();
                }
            }

            ICrawlerMapTypeHelper helper = GetMapHelper(mapData.Map.CrawlerMapTypeId);

            _crawlerMapRoot = await helper.EnterMap(party, mapData, token);

            _crawlerMapRoot.MapTypeHelper = helper;
            await LoadDungeonAssets(_crawlerMapRoot, token);

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

            _screenService.Close(ScreenNames.Loading);
        }

        private async Task LoadDungeonAssets(CrawlerMapRoot mapRoot, CancellationToken token)
        {

            List<long> zoneTypes = mapRoot.GetAllZoneTypes();

            foreach (long zoneTypeId in zoneTypes)
            {
                ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

                if (ztype != null)
                {
                    AssetBlock block = new AssetBlock() { ZoneTypeId = ztype.IdKey };

                    mapRoot.AssetBlocks[zoneTypeId] = block;

                    string dungeonArtName = ztype.Art;

                    _assetService.LoadAsset(AssetCategoryNames.Dungeons, dungeonArtName, OnLoadDungeonAssets, block, null, token);

                }
            }

            string buildingArtFolder = _gameData.Get<BuildingArtSettings>(_gs.ch).Get(mapRoot.Map.BuildingArtId).Art;

            _assetService.LoadAsset(AssetCategoryNames.Buildings, "CityAssets", OnLoadCityAssets, null, null, token, buildingArtFolder);


            while (mapRoot.AssetBlocks.Any(a => !a.Value.IsReady()))
            {
                await Task.Delay(1);
            }
        }
        private void OnLoadCityAssets(object obj, object data, CancellationToken token)
        {
            GameObject assetGo = obj as GameObject;

            if (assetGo == null)
            {
                return;
            }

            _crawlerMapRoot.CityAssets = assetGo.GetComponent<CityAssets>();
        }

        private void OnLoadDungeonAssets(object obj, object data, CancellationToken token)
        {
            GameObject assetGo = obj as GameObject;

            if (assetGo == null)
            {
                return;
            }


            AssetBlock block = data as AssetBlock;

            if (block == null)
            {
                return;
            }

            block.DungeonAssets = assetGo.GetComponent<DungeonAssets>();

            long materialSeed = _crawlerMapRoot.Map.ArtSeed / 5 + 1433 + block.ZoneTypeId;

            int matWeightSum = block.DungeonAssets.Materials.Sum(x => x.Weight);

            int weightChosen = (int)materialSeed % matWeightSum;

            foreach (WeightedDungeonMaterials mat in block.DungeonAssets.Materials)
            {
                weightChosen -= mat.Weight;

                if (weightChosen <= 0)
                {
                    block.DungeonMaterials = mat.Materials;
                    break;
                }
            }

            // Get doormat for this level.

            List<WeightedMaterial> doorMats = block.DungeonMaterials.GetMaterials(DungeonAssetIndex.Doors);

            long doorWeightSum = doorMats.Sum(x => x.Weight);

            long doorHash = _crawlerMapRoot.Map.ArtSeed / 3 + 317;

            long doorChosen = doorHash % doorWeightSum;

            foreach (WeightedMaterial wmat in doorMats)
            {
                doorChosen -= wmat.Weight;
                if (doorChosen <= 0)
                {
                    block.DoorMat = wmat.Mat;
                    break;
                }
            }
        }

        public async Task OnClientResetCleanup(CancellationToken token)
        {
            CleanMap();
            await Task.CompletedTask;
        }


        public void CleanMap()
        {
            if (_crawlerMapRoot != null)
            {
                foreach (AssetBlock block in _crawlerMapRoot.AssetBlocks.Values)
                {

                    if (block.DungeonAssets != null)
                    {
                        _clientEntityService.Destroy(block.DungeonAssets.gameObject);
                        block.DungeonAssets = null;
                    }
                    block.DungeonMaterials = null;

                }
                _crawlerMapRoot.AssetBlocks.Clear();
                if (_crawlerMapRoot.CityAssets != null)
                {
                    _clientEntityService.Destroy(_crawlerMapRoot.CityAssets.gameObject);
                    _crawlerMapRoot.CityAssets = null;
                }
                _clientEntityService.Destroy(_crawlerMapRoot.gameObject);
                _crawlerMapRoot = null;
            }
        }

        public void UpdateCameraPos(CancellationToken token)
        {
            if (_crawlerMapRoot == null)
            {
                return;
            }

            if (_playerLight != null)
            {

                if (InDungeonMap())
                {
                    _playerLight.intensity = 100;
                    _playerLight.range = 1000;
                }
                else
                {
                    _playerLight.intensity = 0;
                }
            }

            if (_camera == null)
            {

                _camera = _cameraController.GetMainCamera();

                // Uncomment for traditional BT layout.

                if (_gs.GameMode == EGameModes.Crawler)
                {
                    // Need this back in for screen scaling.
                    // _camera.rect = new Rect(0, 0, 2f / 3f, 1);
                }

                _camera.transform.localPosition = new Vector3(0, 0, -CrawlerMapConstants.XZBlockSize * 0.6f);
                _camera.transform.eulerAngles = new Vector3(0, 0, 0);
                _camera.farClipPlane = CrawlerMapConstants.XZBlockSize * 8;
                _camera.fieldOfView = 70f;
            }

            _cameraParent.transform.position = new Vector3(_crawlerMapRoot.DrawX, _crawlerMapRoot.DrawY, _crawlerMapRoot.DrawZ);
            _cameraParent.transform.eulerAngles = new Vector3(0, _crawlerMapRoot.DrawRot + 90, 0);
            _dispatcher.Dispatch(new SetWorldPicture(null, false));
            if (_playerLightObject != null && _camera != null)
            {
                _playerLightObject.transform.position = _camera.transform.position;
            }
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

        private void SetMapComplete(PartyData party, CrawlerWorld world, long mapId)
        {

            CrawlerMap map = world.GetMap(mapId);

            if (map == null)
            {
                return;
            }

            _party.CompletedMaps.SetBit(map.IdKey);
            for (int xx = 0; xx < map.Width; xx++)
            {
                for (int zz = 0; zz < map.Height; zz++)
                {
                    long questItemId = map.GetEntityId(xx, zz, EntityTypes.QuestItem);
                    if (questItemId > 0)
                    {
                        _party.QuestItems.SetBit(questItemId);
                    }
                }
            }

            _questService.GiveExploreQuestCredit(party, mapId);
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

            _party.CurrentMap.Cleansed.SetBit(map.GetIndex(x, z));

            MapEncounterType encounterType = _gameData.Get<MapEncounterSettings>(_gs.ch).Get(GetEncounterAtCell(_party, map, x, z));

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

            _party.CurrentMap.Visited.SetBit(index);

            if (map.CrawlerMapTypeId == CrawlerMapTypes.City)
            {
                SetMapComplete(_party, _world, map.IdKey);
                return false;
            }

            if (_party.CompletedMaps.HasBit(mapId))
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

            if (!status.Visited.HasBit(index))
            {
                status.CellsVisited++;
            }

            status.Visited.SetBit(index);

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

                        if (!_party.CompletedMaps.HasBit(cm.IdKey))
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
            x = MathUtils.ModClamp(x, map.Width);
            z = MathUtils.ModClamp(z, map.Height);

            if (x < 0 || x >= map.Width || z < 0 || z >= map.Height)
            {
                return false;
            }

            if (thisRunOnly)
            {
                return _party.CurrentMap.Visited.HasBit(map.GetIndex(x, z));
            }


            if (_party.CompletedMaps.HasBit(mapId))
            {
                return true;
            }


            CrawlerMapStatus status = _party.GetMapStatus(mapId, false);
            if (status == null)
            {
                return false;
            }

            int index = map.GetIndex(x, z);

            return status.Visited.HasBit(index);
        }

        public void MovePartyTo(PartyData party, int x, int z, int rot, bool showMinimap, CancellationToken token)
        {
            if (_crawlerMapRoot == null)
            {
                return;
            }

            x = MathUtils.Clamp(0, x, _crawlerMapRoot.Map.Width - 1);
            z = MathUtils.Clamp(0, z, _crawlerMapRoot.Map.Height - 1);

            _crawlerMapRoot.DrawX = x * CrawlerMapConstants.XZBlockSize;
            _crawlerMapRoot.DrawZ = z * CrawlerMapConstants.XZBlockSize;
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

                        TileImages[i] = new FullWallTileImage() { Index = i, WallIds = vals, RefImage = wti, RotAngle = ((4 - rot) % 4) * 90, };

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
                    TileImages[i] = new FullWallTileImage() { Index = i, WallIds = vals, RefImage = wti };
                }
            }

            StringBuilder outputSb = new StringBuilder();

            for (int i = 0; i < TileImages.Length; i++)
            {
                StringBuilder sb = new StringBuilder();

                for (int w = 0; w < TileImageConstants.WallCount; w++)
                {
                    sb.Append(_wallLetterList[TileImages[i].WallIds[w]]);
                }
            }
        }

        public bool InDungeonMap()
        {
            return _crawlerMapRoot != null && _crawlerMapRoot.Map != null && _crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon;
        }

        public bool IsIndoors()
        {
            return _crawlerMapRoot != null && _crawlerMapRoot.Map != null && _crawlerMapRoot.Map.HasFlag(CrawlerMapFlags.IsIndoors);
        }

        public bool HasMagicBit(int x, int z, long bit, bool modifyWithPartyBuffs)
        {
            return FlagUtils.IsSet(GetMagicBits(_party.CurrPos.MapId, x, z, modifyWithPartyBuffs), (1 << (int)bit));
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

            if (mapId == _party.CurrPos.MapId && _party.CurrentMap.Cleansed.HasBit(map.GetIndex(x, z)))
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

        virtual public long GetEncounterAtCell(PartyData party, CrawlerMap map, int x, int z)
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

            if (etype.CanBeCleansed && party.CurrentMap.Cleansed.HasBit(map.GetIndex(x, z)))
            {
                return 0;
            }


            if (!etype.CanRepeat)
            {
                // Check if map is completed or we have the one-time flag set.
                // If didn't visit now and didn't complete map, then the encounter is there.
                if (party.CompletedMaps.HasBit(map.IdKey))
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

                bool didVisitThisRun = PartyHasVisited(map.IdKey, x, z, true);

                if (didVisitThisRun)
                {
                    return 0;
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
    }
}
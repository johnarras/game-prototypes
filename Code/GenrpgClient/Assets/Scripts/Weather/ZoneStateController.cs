using Assets.Scripts.MapTerrain;
using Assets.Scripts.UI.Entities;
using ClientEvents;
using Assets.Scripts.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.ProcGen.Settings.Weather;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public struct UpdateColor
{
    public UnityEngine.Color Current;
    public UnityEngine.Color Target;

    public void Set(UnityEngine.Color val)
    {
        Current = val;
        Target = val;
    }
}

public struct UpdateFloat
{
    public float Current;
    public float Target;

    public void Set(float val)
    {
        Current = val;
        Target = val;
    }
}

[Serializable]
public class WeatherEffectContainer
{
    public string Name;
    public WeatherType Weather;
}

public interface IZoneStateController : IInitializable
{
    Zone GetCurrentZone();
    long GetCurrentZoneShown();

    Light SunLight { get; }
}

public class ZoneStateController : IZoneStateController
{
    private ICameraController _cameraController = null;
    private IMapTerrainManager _terrainManager = null;
    private IPlayerManager _playerManager = null;
    private IMapProvider _mapProvider = null;
    private IAudioService _audioService = null;
    private IModTextureService _modTextureService = null;
    private ICrawlerService _crawlerService = null;
    private ICrawlerWorldService _worldService = null;
    private IDispatcher _dispatcher = null;
    private IClientUpdateService _updateService = null;
    private IClientGameState _gs = null;
    private IGameData _gameData = null;
    private IScreenService _screenService = null;
    private IClientRandom _rand = null;
    private IInitClient _initClient = null;


    private CancellationToken _token;

    private CoreClientData _coreData = null;

    public async Task Initialize(CancellationToken token)
    {
        _token = token;
        _coreData = _initClient.GetCoreClientData();
        RenderSettings.sun = _coreData.SunLight;
        _updateService.AddUpdate(this, ZoneUpdate, UpdateTypes.Regular, _token);
        _dispatcher.AddListener<OnFinishLoadPlayer>(OnFinishLoadingPlayer, _token);
        ResetColors();
        await Task.CompletedTask;
    }

    private float AmbientScale = 1.0f;
    private float SunlightScale = 1.0f;

    public const float BaseFogStart = 150;
    public const float BaseFogEnd = 300;
    private float FogDistScale = 1.0f;


    public bool PauseUpdates = false;
    public const int MaxTicksBetweenZoneUpdates = 3;


    public float LinearFogEnd = 300;

    private long _currentZoneShown = 0;
    public long GetCurrentZoneShown()
    {
        return _currentZoneShown;
    }

    public Light SunLight => _coreData.SunLight;


    DateTime windBurstEnd = DateTime.UtcNow;
    DateTime nextWindBurst = DateTime.UtcNow;


    public const float WeatherTransitionTime = 20.0f;
    private WeatherType _dataWeather = null;

    public DateTime NextWeatherTransition = DateTime.UtcNow.AddSeconds(1000000);

    public float SunlightIntensityMultiplier = 1.1f;
    public float AmbientIntensityMultiplier = 0.5f;

    public const float ColorFrameDelta = 0.008f;

    public const float FogDensityDelta = 0.0005f;

    public const float FogDistDelta = 1.0f;

    int ticksToZoneUpdate = 0;

    public List<WeatherEffectContainer> Effects;

    public UpdateFloat SunlightIntensity;
    public UpdateFloat FogDensity;
    public UpdateFloat FogStart;
    public UpdateFloat FogEnd;
    public UpdateFloat PrecipScale;
    public UpdateFloat WindScale;
    public UpdateFloat ParticleScale;
    public UpdateFloat CloudDensity;
    public UpdateFloat CloudSpeed;

    public UpdateColor SkyColor;
    public UpdateColor FogColor;
    public UpdateColor SunlightColor;
    public UpdateColor CloudColor;
    public UpdateColor AmbientColor;

    Zone _currentZone;
    ZoneType _currentZoneType;


    protected WeatherType CurrentWeatherType;

    private void ResetColors()
    {
        SunlightIntensity.Set(1.0f);
        FogStart.Set(BaseFogStart);
        FogEnd.Set(BaseFogEnd);
        FogDensity.Set(0.001f);
        CloudSpeed.Set(1.0f);
        PrecipScale.Set(0.0f);
        WindScale.Set(0.0f);
        ParticleScale.Set(0.0f);
        CloudDensity.Set(0.0f);
        CloudColor.Set(Color.gray);
        SkyColor.Set(Color.cyan);
        SunlightColor.Set(Color.white);
        AmbientColor.Set(Color.white);
        FogColor.Set(Color.gray);
        TurnOnFogIfValid();
        SetupSkybox();

    }

    private void TurnOnFogIfValid()
    {
        RenderSettings.fog = false;
    }

    public void SetupSkybox()
    {
        RenderSettings.skybox = _coreData.SkyboxMaterial;
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", UnityEngine.Color.white * 2);
        }
    }

    public Zone GetCurrentZone()
    {
        return _currentZone;
    }

    private void OnFinishLoadingPlayer(OnFinishLoadPlayer edata)
    {
        ResetColors();
        _currentZone = null;
        _currentZoneType = null;

        return;
    }

    private bool UseDynamicWeather() { return _gs.GameMode != EGameModes.MMO; }

    private long _crawlerMapId = 0;
    private bool _didInitZoneState = false;
    private void ZoneUpdate()
    {

        if (!_didInitZoneState)
        {
            GameObject go = _playerManager.GetPlayerGameObject();
            if (go != null || UseDynamicWeather())
            {
                ResetColors();
                _didInitZoneState = true;
            }
        }

        float delta = (UseDynamicWeather() ? 1 : ColorFrameDelta);

        if (AmbientScale < 1.0f)
        {
            delta *= 2;
        }

        --ticksToZoneUpdate;
        if (ticksToZoneUpdate <= 0)
        {
            ticksToZoneUpdate = MaxTicksBetweenZoneUpdates;
            GameObject go = _playerManager.GetPlayerGameObject();
            if (go != null)
            {
                int wx = (int)go.transform.localPosition.x;
                int wy = (int)go.transform.localPosition.z;

                if (wx >= 0 && wy >= 0 && wx < _mapProvider.GetMap().GetHwid() && wy < _mapProvider.GetMap().GetHhgt())
                {

                    int gx = wx / (MapConstants.TerrainPatchSize - 1);
                    int gy = wy / (MapConstants.TerrainPatchSize - 1);


                    int zoneId = 0;
                    TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);
                    if (patch != null && patch.mainZoneIds != null)
                    {
                        wx %= (MapConstants.TerrainPatchSize - 1);
                        wy %= (MapConstants.TerrainPatchSize - 1);
                        zoneId = patch.mainZoneIds[wy, wx];
                    }

                    ActiveScreen hud = _screenService.GetScreen(ScreenNames.HUD);

                    if (((_currentZone == null || _currentZone.IdKey != zoneId) && zoneId > 1) && hud != null)
                    {
                        Zone zone = _mapProvider.GetMap().Get<Zone>(zoneId);
                        if (zone == null)
                        {
                            return;
                        }
                        _currentZone = zone;
                        _currentZoneShown = zone.IdKey;
                        _gs.ch.ZoneId = zone.IdKey;
                        _currentZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(_currentZone.ZoneTypeId);
                        _dataWeather = _gameData.Get<WeatherTypeSettings>(_gs.ch).Get(_currentZoneType.WeatherTypeId);

                    }
                }
            }
            else if (UseDynamicWeather())
            {

                PartyData party = _crawlerService.GetParty();
                if (party != null)
                {
                    long mapID = party.CurrPos.MapId;
                    if (mapID != _crawlerMapId)
                    {
                        CrawlerMap map = _worldService.GetMap(mapID);
                        if (map == null)
                        {
                            _crawlerMapId = 0;
                            return;
                        }
                        _dataWeather = _gameData.Get<WeatherTypeSettings>(_gs.ch).Get(map.WeatherTypeId);

                        if (_dataWeather != null)
                        {
                            _crawlerMapId = mapID;
                        }
                    }
                }
            }

            if (_dataWeather == null)
            {
                return;
            }

            SunlightColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.LightColor);
            FogColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.FogColor);
            CloudColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.CloudColor);
            AmbientColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.AmbientColor);
            SkyColor.Target = _modTextureService.ConvertMyColorToColor(_dataWeather.SkyColor);

            FogDensity.Target = _dataWeather.FogScale;
            CloudSpeed.Target = _dataWeather.CloudSpeed;
            CloudDensity.Target = _dataWeather.CloudScale;
            PrecipScale.Target = _dataWeather.PrecipScale;
            WindScale.Target = _dataWeather.WindScale;
            ParticleScale.Target = _dataWeather.ParticleScale;

            SunlightIntensity.Target = _dataWeather.LightScale;
            if (SunlightIntensityMultiplier > 0)
            {
                SunlightIntensity.Target *= SunlightIntensityMultiplier;
            }

            FogStart.Target = _dataWeather.FogDistance;
            FogEnd.Target = LinearFogEnd;

            _audioService.PlayMusic(_currentZoneType);
            if (FogDistScale <= 1.0f)
            {
                if (_gs.GameMode == EGameModes.MMO)
                {
                    _dispatcher.Dispatch(new SetZoneNameEvent());
                }
            }
        }

        UpdateZoneState(delta);

        UpdateWind();

    }

    private void UpdateZoneState(float delta)
    {
        if (SunlightIntensity.Target < 0.1f)
        {
            SunlightIntensity.Target = 0.1f;
        }

        float fogDensityMult = (FogDistScale > 0 ? 1 / FogDistScale : 1.0f);
        fogDensityMult = 0;
        AmbientColor.Current = _modTextureService.MoveCurrToTargetColor(AmbientColor.Current, AmbientColor.Target * AmbientScale, delta);
        FogColor.Current = _modTextureService.MoveCurrToTargetColor(FogColor.Current, FogColor.Target, delta);
        SunlightColor.Current = _modTextureService.MoveCurrToTargetColor(SunlightColor.Current, SunlightColor.Target, delta);
        SkyColor.Current = _modTextureService.MoveCurrToTargetColor(SkyColor.Current, SkyColor.Target, delta);
        CloudColor.Current = _modTextureService.MoveCurrToTargetColor(CloudColor.Current, CloudColor.Target, delta);
        FogDensity.Current = _modTextureService.MoveCurrFloatToTarget(FogDensity.Current, FogDensity.Target * fogDensityMult, delta * 0.01f);

        if (_cameraController != null)
        {
            List<Camera> allCams = _cameraController.GetAllCameras();
            foreach (Camera cam in allCams)
            {
                cam.backgroundColor = SkyColor.Current;
            }
        }

        FogStart.Current = _modTextureService.MoveCurrFloatToTarget(FogStart.Current, FogStart.Target * FogDistScale, FogDistDelta * FogDistScale);
        FogEnd.Current = _modTextureService.MoveCurrFloatToTarget(FogEnd.Current, FogEnd.Target * FogDistScale, FogDistDelta * FogDistScale);

        SunlightIntensity.Current = _modTextureService.MoveCurrFloatToTarget(SunlightIntensity.Current, SunlightIntensity.Target * SunlightScale, delta);
        CloudSpeed.Current = _modTextureService.MoveCurrFloatToTarget(CloudSpeed.Current, CloudSpeed.Target, delta);
        WindScale.Current = _modTextureService.MoveCurrFloatToTarget(WindScale.Current, WindScale.Target, delta);
        PrecipScale.Current = _modTextureService.MoveCurrFloatToTarget(PrecipScale.Current, PrecipScale.Target, delta);
        ParticleScale.Current = _modTextureService.MoveCurrFloatToTarget(ParticleScale.Current, ParticleScale.Target, delta);
        CloudDensity.Current = _modTextureService.MoveCurrFloatToTarget(CloudDensity.Current, CloudDensity.Target, delta);

        UpdateSettings();
    }

    private void UpdateSettings()
    {
        RenderSettings.ambientSkyColor = AmbientColor.Current * AmbientIntensityMultiplier * 1.05f;
        RenderSettings.ambientEquatorColor = AmbientColor.Current * AmbientIntensityMultiplier * 0.9f;
        RenderSettings.ambientGroundColor = AmbientColor.Current * AmbientIntensityMultiplier * 0.5f;


        RenderSettings.fogColor = FogColor.Current;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = FogStart.Current;
        RenderSettings.fogEndDistance = FogEnd.Current;

        if (_coreData.SunLight != null)
        {
            _coreData.SunLight.intensity = SunlightIntensity.Current * SunlightScale * SunlightIntensityMultiplier;
            _coreData.SunLight.color = SunlightColor.Current;

        }

        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", FogColor.Current * 0.5f);
        }


    }


    private void UpdateWind()
    {
        if (_coreData.Wind == null)
        {
            return;
        }

        if (windBurstEnd < DateTime.UtcNow && _coreData.Wind.windMain > 0)
        {
            _coreData.Wind.windMain = 0.13f * _coreData.Wind.windMain * WindScale.Current;
        }
        if (nextWindBurst < DateTime.UtcNow)
        {
            _coreData.Wind.windMain = RandUtils.FloatRange(0.66f, 1.33f, _rand) * WindScale.Current;
            windBurstEnd = DateTime.UtcNow.AddSeconds(RandUtils.FloatRange(4.0f, 7.0f, _rand));
            nextWindBurst = DateTime.UtcNow.AddSeconds(RandUtils.FloatRange(12.0f, 22.0f, _rand));
        }
    }
}



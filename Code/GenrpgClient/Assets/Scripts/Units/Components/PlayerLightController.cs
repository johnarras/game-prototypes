using OxDb.Client.Crawler.Maps.Services;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Buffs.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using UnityEngine;

namespace OxDb.Client.Controllers
{
    public class PlayerLightController : BaseBehaviour
    {
        private IModTextureService _modTextureService = null;
        private ICrawlerMapService _crawlerMapService = null;
        private ICrawlerService _crawlerService = null;
        private ICrawlerWorldService _crawlerWorldService = null;
        private IClientConfigContainer _configContainer = null;

        public float Range = 75;

        public Light Headlight;

        public float IntensityDelta = 7f;
        public float MaxIntensity = 150;

        float _currIntensity = 0;
        float _targetIntensity = 0;

        public Color LowColor;
        public Color MidColor;
        public Color HighColor;

        public Vector3 Offset;

        public int CombatDarkenTicks = 10;
        public float CombatDarkIntensityScale = 0.25f;

        private int _maxStableTicks = 5;
        private int _stableTicksLeft = 0;

        private int _currentCombatTicks = 0;

        public float FlickerSpeed = 0.001f;

        public override void Init()
        {
            base.Init();
            AddUpdate(LightUpdate, UpdateTypes.Late);
            _targetIntensity = MaxIntensity;
            _currIntensity = MaxIntensity;
            if (_configContainer.Config.GameMode != EGameModes.Crawler && Headlight != null)
            {
                Headlight.intensity = 0;
            }
        }

        private bool UpdateCombatDarkenTicks(bool inCombat)
        {

            if (inCombat)
            {
                if (_currentCombatTicks >= CombatDarkenTicks)
                {
                    return false;
                }
                _currentCombatTicks++;
            }
            else
            {
                if (_currentCombatTicks <= 0)
                {
                    return false;
                }
                _currentCombatTicks--;
            }

            float combatIntensityPercent = 1.0f * _currentCombatTicks / CombatDarkenTicks;

            Headlight.intensity = MaxIntensity * (1.0f - combatIntensityPercent * 0.75f);


            return true;
        }

        bool haveSetPosition = false;
        private void LightUpdate()
        {
            if (!_crawlerMapService.InIndoorMap())
            {
                return;
            }

            PartyData party = _crawlerService.GetParty();
            CrawlerMap map = _crawlerWorldService.GetMap(party.CurrPos.MapId);

            if (_crawlerMapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.Darkness, true))
            {
                party.Buffs[PartyBuffs.Light] = 0;
                Headlight.intensity = 0;
                return;
            }

            if (!haveSetPosition)
            {
                entity.transform.localPosition = Offset;
            }
            haveSetPosition = true;

            if (party.Combat != null)
            {

                UpdateCombatDarkenTicks(true);
                return;
            }
            else
            {
                if (UpdateCombatDarkenTicks(false))
                {
                    return;
                }
            }

            float colorNoise = Mathf.PerlinNoise(Time.time * FlickerSpeed, 0.0f);

            if (colorNoise < 0.5f)
            {
                float localNoise = 2 * colorNoise;
                Headlight.color = Color.Lerp(LowColor, MidColor, localNoise);
            }
            else
            {
                // Re-map t from [0.5, 1.0] to [0.0, 1.0] for the lerp
                float localNoise = (colorNoise - 0.5f) * 2.0f;
                Headlight.color = Color.Lerp(MidColor, HighColor, localNoise);
            }

            if (_currIntensity != _targetIntensity)
            {
                _currIntensity = _modTextureService.MoveCurrFloatToTarget(_currIntensity, _targetIntensity, RandUtils.FloatRange(0, IntensityDelta * 2, _gs.Rand));
            }

            if (_currIntensity == _targetIntensity)
            {
                _stableTicksLeft--;

                if (_stableTicksLeft <= 0)
                {
                    _targetIntensity = RandUtils.FloatRange(MaxIntensity * 3 / 4, MaxIntensity, _gs.Rand);
                    _stableTicksLeft = RandUtils.IntRange(0, _maxStableTicks, _gs.Rand);
                }
            }

            if (Headlight != null)
            {
                float lightTarget = party.Buffs[PartyBuffs.Light];
                if (lightTarget > 1)
                {
                    lightTarget = 1 + Mathf.Log(lightTarget, 2) / 10;
                }
                Headlight.intensity = _currIntensity * lightTarget;
                Headlight.range = Range;
            }
        }
    }
}



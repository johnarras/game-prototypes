using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Buffs.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using UnityEngine;

namespace Assets.Scripts.Controllers
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

        float _currIntensity = 0;
        float _targetIntensity = 0;

        const float IntensityDelta = 7f;

        public float MaxIntensity = 150;

        public Vector3 Offset;

        private int _maxStableTicks = 5;
        private int _stableTicksLeft = 0;

        public float FlickerSpeed = 0.001f;

        Color _color1;
        Color _color2;

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
            _color1 = Headlight.color;
            _color2 = Color.orange;
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

            float noise = Mathf.PerlinNoise(Time.time * FlickerSpeed, 0.0f);

            Headlight.color = Color.Lerp(_color1, _color2, noise);

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



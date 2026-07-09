using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Audio.ClientEvents;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.GameObjects;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Dungeons.Audio
{

    public interface IAmbientSoundsService : IInitializable
    {
    }

    public class FullAmbientSoundList
    {
        public bool DidStartPlaying;
        public string CategoryName;
        public AmbientSoundList SoundList;
        public List<AmbientSoundStatus> Statuses = new List<AmbientSoundStatus>();
    }


    public class AmbientSoundStatus
    {
        public float ElapsedTimeSinceLastPlay { get; set; }
        public AmbientSound Sound { get; set; }
    }

    public class AmbientSoundsService : IAmbientSoundsService
    {
        private const string _anchorName = "AmbientSoundAnchor";
        private static readonly string _assetFilenameSuffix = typeof(AmbientSoundList).Name;


        private IClientUpdateService _updateService = null;
        private IClientGameState _gs = null;
        private IClientAppService _appService = null;
        private IDispatcher _dispatcher = null;
        private ICrawlerMapService _mapService = null;
        private ICrawlerService _crawlerService = null;
        private ISingletonContainer _singletonContainer = null;
        private IAwaitableService _awaitableService = null;
        private IAssetService _assetService = null;

        private GameObject _ambientAnchor;

        private CancellationToken _token;

        private Dictionary<string, FullAmbientSoundList> _soundCache = new Dictionary<string, FullAmbientSoundList>();

        private void OnSetAmbientSoundCategory(SetAmbientSoundCategory setCategory)
        {
            _awaitableService.ForgetAwaitable(SetSoundCategoryAsync(setCategory.CategoryName));
        }


        private async Awaitable SetSoundCategoryAsync(string categoryName)
        {
            if (categoryName == _currName)
            {
                return;
            }

            StopSounds();

            if (string.IsNullOrEmpty(categoryName))
            {
                return;
            }

            if (!_soundCache.TryGetValue(categoryName, out FullAmbientSoundList fullSoundList))
            {

                AmbientSoundList soundList = await _assetService.LoadAssetAsync<AmbientSoundList>(AssetCategoryNames.Audio,
                    categoryName + _assetFilenameSuffix, _ambientAnchor, _token);

                if (soundList != null)
                {
                    fullSoundList = new FullAmbientSoundList() { CategoryName = categoryName, SoundList = soundList };

                    foreach (AmbientSound sound in soundList.Sounds)
                    {
                        fullSoundList.Statuses.Add(new AmbientSoundStatus()
                        {
                            Sound = sound,
                        });
                    }
                    _soundCache[categoryName] = fullSoundList;
                }
            }

            if (fullSoundList != null)
            {
                _currName = categoryName;
                _currList = fullSoundList;
                _currList.DidStartPlaying = false;
            }
        }

        public void StopSounds()
        {
            foreach (FullAmbientSoundList fullList in _soundCache.Values)
            {
                fullList.DidStartPlaying = false;
                foreach (AmbientSoundStatus status in fullList.Statuses)
                {
                    _dispatcher.Dispatch(new StopSound(status.Sound.SoundName));
                }
            }
            _currList = null;
            _currName = null;
        }

        private string _currName = null;
        private FullAmbientSoundList _currList = null;
        public async Task Initialize(CancellationToken token)
        {
            _ambientAnchor = _singletonContainer.GetSingleton(_anchorName);
            _updateService.AddUpdate(this, UpdateAmbientSounds, UpdateTypes.Regular, token);
            _dispatcher.AddListener<SetAmbientSoundCategory>(OnSetAmbientSoundCategory, token);
            _token = token;
            await Task.CompletedTask;
        }

        private void UpdateAmbientSounds()
        {
            if (string.IsNullOrEmpty(_currName) || _currList == null)
            {
                return;
            }

            CrawlerMapRoot mapRoot = _mapService.GetMapRoot();

            PartyData party = _crawlerService.GetParty();

            float tickTime = _appService.GetDeltaTime();
            DateTime currTime = DateTime.UtcNow;
            foreach (AmbientSoundStatus status in _currList.Statuses)
            {
                if (status.Sound.MinSecondsBetweenProc == 0)
                {
                    if (!_currList.DidStartPlaying)
                    {
                        _dispatcher.Dispatch(new PlaySound(status.Sound.SoundName));
                    }
                    continue;
                }

                status.ElapsedTimeSinceLastPlay += tickTime;

                if (status.ElapsedTimeSinceLastPlay < status.Sound.MinSecondsBetweenProc)
                {
                    continue;
                }

                if (_gs.Rand.NextDouble() < status.Sound.ProcsPerSecond / _appService.TargetFrameRate)
                {
                    _dispatcher.Dispatch(new PlaySound(status.Sound.SoundName));
                    status.ElapsedTimeSinceLastPlay = 0;
                }
            }

            _currList.DidStartPlaying = true;
        }
    }
}

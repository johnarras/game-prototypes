using Assets.Scripts.Audio.Constants;
using Assets.Scripts.Crawler.Shared.GameEvents;
using Assets.Scripts.Options.Services;
using Assets.Scripts.UI.Abstractions;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Options
{
    public class ClientOptionsScreen : BaseScreen
    {
        protected IAudioService _audioService = null;
        protected ICrawlerService _crawlerService = null;
        protected IClientOptionsService _optionsService = null;
        protected IClientAppService _appService = null;

        public GToggle FullScreenToggle;
        public GToggle PauseAtEndOfCombatToggle;
        public GToggle ClassicMovementButtons;

        public GSlider MusicVolumeSlider;
        public GSlider SoundVolumeSlider;
        public GSlider AmbientVolumeSlider;
        public GSlider TextScrollSpeedSlider;

        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            Dictionary<EAudioCategories, GSlider> audioSliders = new Dictionary<EAudioCategories, GSlider>();

            audioSliders[EAudioCategories.Sound] = SoundVolumeSlider;
            audioSliders[EAudioCategories.Music] = MusicVolumeSlider;
            audioSliders[EAudioCategories.Ambient] = AmbientVolumeSlider;

            foreach (EAudioCategories category in audioSliders.Keys)
            {
                _uiService.SetSlider(audioSliders[category],
                    AudioConstants.MinVolume, AudioConstants.MaxVolume, _audioService.GetVolume(category),
                    (float value) => { _audioService.SetVolume(category, value); });
            }

            _uiService.SetSlider(TextScrollSpeedSlider, 1, CrawlerCombatConstants.ScrollingFramesValues.Length,
            _crawlerService.GetParty()?.ScrollFramesIndex ?? 1, (float newValue) =>
            {
                if (_crawlerService.GetParty() != null)
                {
                    _crawlerService.GetParty().ScrollFramesIndex = (int)newValue;
                }
            });

            FullScreenToggle?.SetIsOn(_appService.IsFullScreen());
            _uiService.SetToggle(FullScreenToggle, ToggleFullScreen);

            ClassicMovementButtons.SetIsOn(_optionsService.GetOptions().HasFlag(ClientFlags.ClassicMovement));
            _uiService.SetToggle(ClassicMovementButtons, ToggleClassicMovement);

            PartyData party = _crawlerService.GetParty();
            if (party == null)
            {
                _clientEntityService.SetActive(PauseAtEndOfCombatToggle, false);
            }
            else
            {
                _clientEntityService.SetActive(PauseAtEndOfCombatToggle, true);
                PauseAtEndOfCombatToggle?.SetIsOn(party.HasFlag(PartyFlags.PauseAtEndOfCombat));
                _uiService.SetToggle(PauseAtEndOfCombatToggle, TogglePauseAtEndOfCombat);
            }

            await Task.CompletedTask;
        }

        private void ToggleClassicMovement(bool isOn)
        {
            if (isOn)
            {
                _optionsService.GetOptions().AddFlags(ClientFlags.ClassicMovement);
            }
            else
            {
                _optionsService.GetOptions().RemoveFlags(ClientFlags.ClassicMovement);
            }
            _dispatcher.Dispatch(new SetupMovementButtons());
        }

        protected override void OnStartClose()
        {
            _optionsService.SaveOptions();
            base.OnStartClose();
        }

        private void TogglePauseAtEndOfCombat(bool isOn)
        {

            PartyData party = _crawlerService.GetParty();
            if (party != null)
            {
                if (isOn)
                {
                    party.AddFlags(PartyFlags.PauseAtEndOfCombat);
                }
                else
                {
                    party.RemoveFlags(PartyFlags.PauseAtEndOfCombat);
                }
            }
        }

        private void ToggleFullScreen(bool isOn)
        {
            _appService.SetFullScreen(!_appService.IsFullScreen());
        }
    }
}


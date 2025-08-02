
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Crawler.Combat;
using Assets.Scripts.Crawler.UI.StatusUI;
using Assets.Scripts.UI.CombatTexts;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Services;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.StatusUI
{


    public class PartyMemberStatusRow : BaseBehaviour
    {
        private IStatusEffectService _statusEffectService;
        private ICrawlerService _crawlerService;
        private ITrainingService _trainingService;

        public GButton Button;
        public FastCombatTextUI FastTextUI;
        public GameObject PortraitParent;
        public AnimatedSprite Portrait;

        public GameObject Root;

        public ProgressBar HealthBar;
        public ProgressBar ManaBar;

        public StatusEffectsUI EffectsUI;
        public CombatEffectUI CombatUI;

        public GText NameText;

        private PartyMember _partyMember = null;
        private PartyData _party = null;
        private int _memberIndex = 0;

        public GImage LevelUpImage;

        public void SetData(int memberIndex)
        {
            _memberIndex = memberIndex;
        }

        public override void Init()
        {
            _updateService.AddUpdate(this, OnLateUpdate, UpdateTypes.Late, GetToken());
            _uiService.SetButton(Button, GetType().Name, ClickPartyMember);
            _dispatcher.AddListener<ShowCombatText>(OnShowCombatText, GetToken());
            UpdateData();
        }

        public PartyMember GetPartyMember()
        {
            return _partyMember;
        }

        private void ClickPartyMember()
        {
            _partyMember = _crawlerService.GetParty().GetMemberInSlot(_memberIndex);

            if (_partyMember == null)
            {
                return;
            }

            _dispatcher.Dispatch(new CrawlerCharacterScreenData() { Unit = _partyMember });
        }

        private bool _needToUpdate = false;
        private long _nextElementTypeId = 0;
        public void UpdateData(long elementTypeId = 0)
        {
            _needToUpdate = true;
            if (_nextElementTypeId == 0)
            {
                _nextElementTypeId = elementTypeId;
            }
        }

        private void OnLateUpdate()
        {
            if (_needToUpdate)
            {
                UpdateDataInternal();
                _needToUpdate = false;
                _nextElementTypeId = 0;
            }
        }

        private void OnShowCombatText(ShowCombatText text)
        {
            if (_memberIndex > 0 && _partyMember != null && _partyMember.Id == text.TargetUnitId)
            {
                UpdateDataInternal();
            }
        }


        private void UpdateDataInternal()
        {
            if (_memberIndex == 0)
            {
                return;
            }
            _party = _crawlerService.GetParty();
            _partyMember = _party.GetMemberInSlot(_memberIndex);

            if (_partyMember == null)
            {
                _clientEntityService.SetActive(Root, false);
                FastTextUI?.SetUnitId(null);
                return;
            }
            else
            {
                FastTextUI?.SetUnitId(_partyMember.Id);

                _clientEntityService.SetActive(Root, true);
                _uiService.SetText(NameText, _partyMember.Name);

                CombatUI?.SetData(_partyMember.Id, Portrait.AnimatedImage, PortraitParent, _partyMember.FactionTypeId);

                long currHp = _partyMember.Stats.Curr(StatTypes.Health);
                long maxHp = _partyMember.Stats.Max(StatTypes.Health);

                HealthBar?.InitRange(0, _partyMember.Stats.Max(StatTypes.Health), _partyMember.Stats.Curr(StatTypes.Health));
                ManaBar?.InitRange(0, _partyMember.Stats.Max(StatTypes.Mana), _partyMember.Stats.Curr(StatTypes.Mana));
                EffectsUI.InitData(_partyMember);
                SetPortrait(_partyMember.PortraitName);

                TrainingInfo info = _trainingService.GetTrainingInfo(_party, _partyMember);

                _clientEntityService.SetActive(LevelUpImage, info.ExpLeft < 1);
            }
        }

        private void SetPortrait(string portraitName)
        {
            Portrait?.SetImage(portraitName);
        }
    }
}

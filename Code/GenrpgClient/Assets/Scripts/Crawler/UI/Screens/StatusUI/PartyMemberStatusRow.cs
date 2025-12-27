
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Crawler.UI.Units;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.StatusUI
{


    public class PartyMemberStatusRow : BaseUnitUI
    {
        protected ICrawlerService _crawlerService = null;
        protected ITrainingService _trainingService = null;
        protected IRoleService _roleService = null;

        public GButton Button;
        public GameObject PortraitParent;
        public AnimatedSprite Portrait;

        public GameObject Root;

        public ProgressBar HealthBar;
        public ProgressBar ManaBar;

        public GText NameText;
        public GText LevelText;

        private PartyMember _partyMember = null;
        private PartyData _party = null;
        private int _memberIndex = 0;

        public GImage LevelUpImage;
        public GImage GuardianImage;

        public void SetData(int memberIndex)
        {
            _memberIndex = memberIndex;
        }

        public override void Init()
        {
            AddUpdate(OnLateUpdate, UpdateTypes.Late);
            _uiService.SetButton(Button, name, ClickPartyMember);
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
                FastCombatTextUI?.SetUnitId(null);
                return;
            }
            else
            {
                FastCombatTextUI?.SetUnitId(_partyMember.Id);

                _clientEntityService.SetActive(Root, true);
                _uiService.SetText(NameText, _partyMember.Name);
                _uiService.SetText(LevelText, _partyMember.Level.ToString());

                CombatEffectUI?.SetData(_partyMember.Id, Portrait.AnimatedImage, PortraitParent, _partyMember.FactionTypeId);

                long currHp = _partyMember.Stats.Curr(StatTypes.Health);
                long maxHp = _partyMember.Stats.Max(StatTypes.Health);

                HealthBar?.InitRange(0, _partyMember.Stats.Max(StatTypes.Health), _partyMember.Stats.Curr(StatTypes.Health));
                ManaBar?.InitRange(0, _partyMember.Stats.Max(StatTypes.Mana), _partyMember.Stats.Curr(StatTypes.Mana));
                StatusEffectsUI?.InitData(_partyMember);
                SetPortrait(_partyMember.PortraitName);

                TrainingInfo info = _trainingService.GetTrainingInfo(_party, _partyMember);

                _clientEntityService.SetActive(LevelUpImage, info.ExpLeft < 1);

                List<Role> userRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(_partyMember.Roles);

                _clientEntityService.SetActive(GuardianImage, userRoles.FastAny(x => x.Guardian));

            }
        }

        private void SetPortrait(string portraitName)
        {
            Portrait?.SetImage(portraitName);
        }
    }
}



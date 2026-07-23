
using OxDb.Client.Assets.Textures;
using OxDb.Client.Crawler.Shared.GameEvents;
using OxDb.Client.Crawler.UI.Units;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Services;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.States.StateHelpers.Exploring;
using OxDb.SharedGame.Crawler.Training.Services;
using OxDb.SharedGame.Stats.Constants;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.UI.Crawler.StatusUI
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
        public ProgressBar ExpBar;

        public GText NameText;
        public GText LevelText;

        private PartyMember _partyMember = null;
        private PartyData _party = null;
        private int _memberIndex = 0;

        public GImage LevelUpImage;
        public GImage GuardianImage;


        private Action _clickAction = null;

        public void SetData(int memberIndex)
        {
            _memberIndex = memberIndex;
        }

        public override void Init()
        {
            AddUpdate(OnLateUpdate, UpdateTypes.Late);
            _uiService.SetButton(Button, name, ClickPartyMember);
            _dispatcher.AddListener<ShowCombatText>(OnShowCombatText, GetToken());
            _dispatcher.AddListener<ClearSelectCrawlerUnitActions>(OnClearSelectCrawlerActions, GetToken());
            _dispatcher.AddListener<SelectPartyMemberIconAction>(OnSelectPartyMemberIcon, GetToken());
            UpdateData();
        }

        private void OnClearSelectCrawlerActions(ClearSelectCrawlerUnitActions clear)
        {
            _clickAction = null;
        }

        public PartyMember GetPartyMember()
        {
            return _partyMember;
        }

        private void OnSelectPartyMemberIcon(SelectPartyMemberIconAction action)
        {
            if (action.Member == _partyMember)
            {
                _clickAction = action.ClickAction;
            }
        }

        private void ClickPartyMember()
        {
            _partyMember = _crawlerService.GetParty().GetMemberInSlot(_memberIndex);



            if (_partyMember == null)
            {
                _clickAction = null;
                return;
            }

            if (_clickAction != null)
            {
                _clickAction.Invoke();
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

                HealthBar?.InitRange(0, _partyMember.Stats.Curr(StatTypes.Health), _partyMember.Stats.Max(StatTypes.Health));
                ManaBar?.InitRange(0, _partyMember.Stats.Curr(StatTypes.Mana), _partyMember.Stats.Max(StatTypes.Mana));
                StatusEffectsUI?.SetData(_partyMember);
                SetPortrait(_partyMember.PortraitName);

                TrainingInfo info = _trainingService.GetTrainingInfo(_party, _partyMember);

                _clientEntityService.SetActive(LevelUpImage, info.ExpLeft < 1);

                ExpBar?.InitRange(0, _partyMember.Exp, info.TotalExp);

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



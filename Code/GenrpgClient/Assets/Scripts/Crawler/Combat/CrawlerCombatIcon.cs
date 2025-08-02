using Assets.Scripts.Assets.Textures;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.CombatTexts;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatIcon : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        private ICrawlerService _crawlerService = null;

        public GameObject IconParent;
        public AnimatedSprite Icon;
        public GText Name;
        public GText Quantity;
        public GText Dist;
        public CombatGroup Group;
        public FastCombatTextUI TextUI;
        public GButton Button;
        public CombatEffectUI EffectUI;

        private Action _clickAction;

        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<SetCombatGroupAction>(OnSetCombatGroupAction, GetToken());
            _dispatcher.AddListener<ClearCombatGroupActions>(OnClearCombatGroupActions, GetToken());
            _uiService.SetButton(Button, GetType().Name, OnClickButton);

        }

        public void UpdateData()
        {
            if (Group == null)
            {
                return;
            }

            if (TextUI != null)
            {
                TextUI.SetGroupId(Group.Id);
            }

            if (EffectUI != null)
            {
                EffectUI.SetData(Group.Id, Icon.AnimatedImage, IconParent, Group.FactionTypeId);
            }

            UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(Group.UnitTypeId);

            if (unitType == null)
            {
                return;
            }
            int okUnitCount = Group.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).Count();

            _uiService.SetText(Name, (okUnitCount == 1 ? Group.SingularName : Group.PluralName));
            _uiService.SetText(Dist, Group.Range + "'");
            _uiService.SetText(Quantity, "x" + okUnitCount);
            Icon.SetImage(unitType.Icon);

        }

        private void OnClickButton()
        {
            _clickAction?.Invoke();
        }

        private void OnClearCombatGroupActions(ClearCombatGroupActions clear)
        {
            _clickAction = null;
        }

        private void OnSetCombatGroupAction(SetCombatGroupAction setAction)
        {

            if (Group != null && Group == setAction.Group)
            {
                _clickAction = setAction.Action;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Group == null || Group.Units.Count < 1)
            {
                return;
            }
            if (_crawlerService.GetState() == ECrawlerStates.ProcessCombatRound)
            {
                return;
            }

            _dispatcher.Dispatch(new ShowInfoPanelEvent() { EntityTypeId = EntityTypes.Unit, EntityId = Group.UnitTypeId });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
        }
    }
}

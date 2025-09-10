using Assets.Scripts.Assets.Textures;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.UI.Units;
using Assets.Scripts.UI.CombatTexts;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.Combat
{
    public class CrawlerCombatIcon : BaseUnitUI, IPointerEnterHandler, IPointerExitHandler
    {

        private ICrawlerService _crawlerService = null;

        public GameObject IconParent;
        public AnimatedSprite Icon;
        public GText Name;
        public GText Quantity;
        public GText Dist;
        public CombatGroup Group;
        public GButton Button;

        private Action _clickAction;

        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<SetCombatGroupAction>(OnSetCombatGroupAction, GetToken());
            _dispatcher.AddListener<ClearCombatGroupActions>(OnClearCombatGroupActions, GetToken());
            _uiService.SetButton(Button, GetType().Name, OnClickButton);

        }

        private bool _didInit = false;
        public void UpdateData()
        {
            if (Group == null || Group.UnitType == null)
            {
                return;
            }

            if (FastCombatTextUI != null)
            {
                FastCombatTextUI.SetGroupId(Group.Id);
            }

            if (CombatEffectUI != null)
            {
                CombatEffectUI.SetData(Group.Id, Icon.AnimatedImage, IconParent, Group.FactionTypeId);
            }

            int okUnitCount = Group.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).Count();

            _uiService.SetText(Name, (okUnitCount == 1 ? Group.SingularName : Group.PluralName));
            _uiService.SetText(Dist, Group.Range + "'");
            _uiService.SetText(Quantity, "x" + okUnitCount);
            if (!_didInit)
            {
                Icon.SetImage(Group.UnitType.Icon);
            }
            _didInit = true;
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

            _dispatcher.Dispatch(new ShowInfoPanelEvent() { EntityTypeId = EntityTypes.Unit, EntityId = Group.UnitType.IdKey });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
        }
    }
}

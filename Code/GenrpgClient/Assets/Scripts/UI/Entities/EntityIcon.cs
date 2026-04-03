using Assets.Scripts.Assets.Sprites.Services;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Interfaces;
using Genrpg.Shared.Effects.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Entities.UI
{
    public class EntityIcon : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler, IEntityQuantityIcon
    {

        protected ISpriteService _spriteService = null;
        protected IEntityService _entityService = null;

        public GImage Icon;
        public GText QuantityText;
        public GText NameText;
        public bool IsMainIcon = false;
        public long UpdateTicks = 10;

        protected long _entityTypeId;
        protected long _entityId;
        protected long _maxQuantity;
        protected long _startQuantity;
        protected long _currQuantity;
        protected long _targetQuantity;
        protected long _ticksSinceUpdate = 0;

        public long EntityTypeId => _entityTypeId;
        public long EntityId => _entityId;
        public long Quantity => _currQuantity;
        public long MaxQuantity => _maxQuantity;

        protected virtual bool IsDooberTarget => true;
        public override void OnReturn()
        {
            base.OnReturn();
            Icon.SetSingleSprite(null);
            _uiService.SetText(QuantityText, null);
            _uiService.SetText(NameText, null);
        }

        public void SetEntityData(IEffect reward, long maxQuantity = 0)
        {
            SetEntityData(reward.EntityTypeId, reward.EntityId, reward.Quantity, maxQuantity);
        }

        virtual protected GameObject GetDooberHitPosition() { return gameObject; }

        public virtual void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            _entityTypeId = entityTypeId;
            _entityId = entityId;
            _maxQuantity = maxQuantity;
            _startQuantity = quantity;
            _currQuantity = _startQuantity;
            _targetQuantity = _startQuantity;
            _ticksSinceUpdate = UpdateTicks;
            AddUpdate(UpdateQuantity, UpdateTypes.Late);
            if (IsDooberTarget)
            {
                _dispatcher.Dispatch(new SetDooberTarget(EntityTypeId, EntityId, GetDooberHitPosition(), IsMainIcon, this));
            }

            _spriteService.SetEntityIcon(entityTypeId, entityId, Icon, GetToken());

            if (maxQuantity < 1)
            {
                _uiService.SetText(QuantityText, quantity.ToString());
            }
            else
            {
                _uiService.SetText(QuantityText, quantity + "/" + maxQuantity);
            }

            if (NameText != null)
            {
                IIdName idname = _entityService.Find(_gs.ch, _entityTypeId, _entityId);
                _uiService.SetText(NameText, idname?.Name ?? string.Empty);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            ShowTooltip(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ShowTooltip(false);
        }

        protected virtual void ShowTooltip(bool visible)
        {
            if (visible)
            {
                _dispatcher.Dispatch(new ShowInfoPanelEvent() { EntityTypeId = _entityTypeId, EntityId = _entityId });
            }
            else
            {
                _dispatcher.Dispatch(new HideInfoPanelEvent());
            }
        }


        protected virtual void ShowQuantity()
        {
            _uiService.SetText(QuantityText, StrUtils.PrintCommaValue(_currQuantity));
        }

        protected virtual void OnUpdateQuantity(bool instantUpdate)
        {
            if (instantUpdate)
            {
                _startQuantity = _targetQuantity;
                _currQuantity = _targetQuantity;
            }
            else
            {
                _startQuantity = _currQuantity;
            }
            _ticksSinceUpdate = UpdateTicks;
            ShowQuantity();
        }

        protected virtual void UpdateQuantity()
        {
            _ticksSinceUpdate++;
            if (_ticksSinceUpdate > UpdateTicks)
            {
                _ticksSinceUpdate = UpdateTicks;
            }

            if (_currQuantity == _targetQuantity)
            {
                return;
            }

            if (UpdateTicks < 2)
            {
                _currQuantity = _targetQuantity;
            }
            else
            {
                _currQuantity = (_targetQuantity - _startQuantity) * _ticksSinceUpdate / UpdateTicks + _startQuantity;
            }

            ShowQuantity();
        }

        public void AddVisualQuantity(long entityTypeId, long entityId, long quantityAdded, bool instant)
        {
            if (entityTypeId != _entityTypeId || entityId != _entityId)
            {
                return;
            }
            _targetQuantity += quantityAdded;
            OnUpdateQuantity(instant);
        }
    }
}



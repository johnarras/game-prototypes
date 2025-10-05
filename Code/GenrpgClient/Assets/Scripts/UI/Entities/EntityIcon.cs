using Assets.Scripts.ClientEvents;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Entities.UI
{
    public class EntityIcon : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        protected IEntityService _entityService = null;

        public GImage Icon;
        public GText QuantityText;
        public GText NameText;

        protected long _entityTypeId;
        protected long _entityId;
        protected long _quantity;
        protected long _maxQuantity;

        public long EntityTypeId => _entityTypeId;
        public long EntityId => _entityId;
        public long Quantity => _quantity;
        public long MaxQuantity => _maxQuantity;

        public void SetEntityData(IReward reward, long maxQuantity = 0)
        {
            SetEntityData(reward.EntityTypeId, reward.EntityId, reward.Quantity, maxQuantity);
        }

        public void SetEntityData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {

            _entityTypeId = entityTypeId;
            _entityId = entityId;
            _quantity = quantity;
            _maxQuantity = maxQuantity;

            _assetService.LoadEntityIcon(entityTypeId, entityId, Icon, GetToken());

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

        private void ShowTooltip(bool visible)
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
    }
}

using Assets.Scripts.ClientEvents.UserCoins;
using Assets.Scripts.Doobers.Events;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;

namespace Assets.Scripts.UI.MobileGame
{
    public class UserCoinDisplay : BaseBehaviour
    {
        public bool IsMainIcon = false;
        public long UpdateTicks = 10;

        public GText QuantityText;

        public GImage Icon;

        public EntityTypeWithIdUI EntityUI;

        private long _startQuantity;
        private long _currQuantity;
        private long _targetQuantity;
        private long _ticksSinceUpdate = 0;
        
        public override void Init()
        {
            _updateService.AddUpdate(this, UpdateQuantity, UpdateTypes.Late, GetToken());
            AddListener<AddUserCoinVisual>(OnAddUserCoinVisual);

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            _startQuantity = userData.Coins.Get(EntityUI.EntityId);
            _currQuantity = _startQuantity;
            _targetQuantity = _startQuantity;
            _ticksSinceUpdate = UpdateTicks;

            if (IsMainIcon)
            {
                _dispatcher.Dispatch(new SetDooberTarget()
                {
                    EntityTypeId = EntityUI.EntityTypeId,
                    EntityId = EntityUI.EntityId,
                    Target = gameObject,
                });
            }

            _assetService.LoadEntityIcon(EntityUI.EntityTypeId, EntityUI.EntityId, Icon, GetToken());
            ShowQuantity();
        }

        private void ShowQuantity()
        {
            _uiService.SetText(QuantityText, StrUtils.PrintCommaValue(_currQuantity));
        }

        private void OnAddUserCoinVisual(AddUserCoinVisual visual)
        {

            if (visual.UserCoinTypeId != EntityUI.EntityId)
            {
                return;
            }

            _targetQuantity += visual.QuantityAdded;

            if (visual.InstantUpdate)
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

        private void UpdateQuantity()
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

            _currQuantity = (_targetQuantity - _startQuantity) * _ticksSinceUpdate / UpdateTicks + _startQuantity;

            ShowQuantity();
        }
    }
}

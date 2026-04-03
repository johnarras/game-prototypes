using Assets.Scripts.Assets.Sprites.Services;
using Assets.Scripts.Entities.UI;
using Assets.Scripts.Trader.Currencies.UI;
using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Settings;

namespace Assets.Scripts.Trader.Caravans.UI
{
    public class CaravanMemberInitIconData
    {
        public bool InCaravan { get; set; }
        public CaravanScreen Screen { get; set; }
        public CaravanMember CaravanMember { get; set; }
    }

    public class CaravanMemberIcon : BaseBehaviour
    {

        protected ISpriteService _spriteService = null;
        protected IClientWebService _webService = null;

        public GImage Icon;
        public GText NameText;
        public GText DescText;
        public EntityIcon SizeIcon;
        public EntityIcon SpeedIcon;

        public GButton MoveButton;
        public GText ButtonText;

        public EntityTypeIconList BonusList;

        private CaravanMemberInitIconData _initData = null;

        private int _siblingIndex = 0;


        public void SetData(CaravanMemberInitIconData initData, int siblingIndex)
        {

            _initData = initData;   
            if (initData.InCaravan)
            {
                _uiService.SetText(ButtonText, "To Holdings");
            }
            else
            {
                _uiService.SetText(ButtonText, "To Caravan");
            }

            _uiService.SetButton(MoveButton, GetName(), MoveCaravanMember);

            _spriteService.SetEntityIcon(EntityTypes.CaravanMember, initData.CaravanMember.IdKey, Icon, GetToken());

            _uiService.SetText(NameText, initData.CaravanMember.Name);
            _uiService.SetText(DescText, initData.CaravanMember.Desc);

            SizeIcon.SetEntityData(EntityTypes.GameplayStat, GameplayStats.MaxSize, -initData.CaravanMember.Size);
            SpeedIcon.SetEntityData(EntityTypes.GameplayStat, GameplayStats.BonusSpeed, initData.CaravanMember.Speed);

            BonusList.ShowEffectList(initData.CaravanMember.Effects);
        }

        private void MoveCaravanMember()
        {
            _initData.Screen.MoveCaravanMember(_initData.CaravanMember.IdKey);
        }

        public long GetCaravanMemberId()
        {
            return _initData.CaravanMember.IdKey;
        }

        public int GetSiblingIndex()
        {
            return _siblingIndex;
        }
    }
}

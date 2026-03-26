using Assets.Scripts.Assets.Sprites.Services;
using Assets.Scripts.Entities.UI;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Currencies.UI;
using Assets.Scripts.Trader.Travel.UI;
using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Trader.CaravanMembers.WebApi;
using Genrpg.Shared.Trader.Cities.Settings;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using NUnit.Framework;

namespace Assets.Scripts.Trader.Caravans.UI
{
    public enum ECaravanMemberLocations
    {
        None,
        Vendor,
        Caravan,
        Holdings,
        Unavailable,
    }

    public class CaravanMemberInitIconData
    {
        public CaravanMember CaravanMember { get; set; }
        public ECaravanMemberLocations CurrentLocation { get; set; }
        public ECaravanMemberLocations TargetLocation { get; set; }
        public SpendType SpendType { get; set; }
        public SpendLocation SpendLoc { get; set; }
        public City CurrentCity { get; set; }
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

        public GText ActionButtonText;
        public GButton ActionButton;

        public SpendCurrencyButton SpendButton;

        public EntityTypeIconList BonusList;

        private CaravanMemberInitIconData _initData = null;

        private int _siblingIndex = 0;

        public void SetData(CaravanMemberInitIconData initData, int siblingIndex)
        {
            _initData = initData;


            _uiService.SetButton(ActionButton, GetName(), OnClickAction);

            if (initData.SpendLoc != null && initData.SpendType != null)
            {
                _clientEntityService.SetActive(SpendButton, true);
                _clientEntityService.SetActive(ActionButton, false);
                SpendButton.SetSpendType(initData.SpendLoc, initData.SpendType);
            }
            else
            {
                _clientEntityService.SetActive(SpendButton, false);
                _clientEntityService.SetActive(ActionButton, true);

                
                if (_initData.TargetLocation == ECaravanMemberLocations.Caravan)
                {
                    _uiService.SetText(ActionButtonText, "Add To Caravan");
                }
                else if (_initData.TargetLocation == ECaravanMemberLocations.Holdings)
                {
                    _uiService.SetText(ActionButtonText, "Remove From Caravan");
                }
                else
                {
                    _clientEntityService.SetActive(ActionButton, false);
                }
            }

            _spriteService.SetEntityIcon(EntityTypes.CaravanMember, initData.CaravanMember.IdKey, Icon, GetToken());

            _uiService.SetText(NameText, initData.CaravanMember.Name);
            _uiService.SetText(DescText, initData.CaravanMember.Desc);

            SizeIcon.SetEntityData(EntityTypes.GameplayStat, GameplayStats.MaxSize, -initData.CaravanMember.Size);
            SpeedIcon.SetEntityData(EntityTypes.GameplayStat, GameplayStats.BonusSpeed, initData.CaravanMember.Speed);

            BonusList.ShowEffectList(initData.CaravanMember.Effects);

            _dispatcher.Dispatch(new UpdateTraderHUD());
        }

        public ECaravanMemberLocations GetCurrentLocation()
        {
            return _initData.CurrentLocation;   
        }

        public long GetCaravanMemberId()
        {
            return _initData.CaravanMember.IdKey;
        }

        public int GetSiblingIndex()
        {
            return _siblingIndex; 
        }

        private void OnClickAction()
        {
            _logService.Info("Clicked Action!");

            if (_initData.TargetLocation == ECaravanMemberLocations.Caravan)
            {
                _webService.SendWebRequest(new AddCaravanMemberToCaravanRequest() { CaravanMemberId = _initData.CaravanMember.IdKey }, GetToken());
            }
        }
    }
}

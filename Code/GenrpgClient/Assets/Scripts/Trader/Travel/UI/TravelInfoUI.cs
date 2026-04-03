using Assets.Scripts.Entities.UI;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.UI.Entities;
using Genrpg.Shared.Attributes.Constants;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Utils.Data;
using UnityEngine;

namespace Assets.Scripts.Trader.Travel.UI
{
    public class TravelInfoUI : BaseBehaviour
    {

        private ICaravanService _caravanService = null;

        public GText DiceCountText;
        public GText BonusDistanceText;

        public EntityTypeIconList DailyCurrencyIcons;
        public EntityTypeIconList TotalCurrencyIcons;

        public CapacityEntityIcon SizeIcon;
        public CapacityEntityIcon InventoryIcon;

        public override void Init()
        {
            _dispatcher.AddListener<UpdateTraderHUD>(OnUpdateVisualTravelStats, GetToken());
            base.Init();
            ShowData();
        }


        private void OnUpdateVisualTravelStats(UpdateTraderHUD response)
        {
            ShowData();
        }


        string rationsSpriteString = null;
        private void ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            ShowExplicitData(coreData);
        }

        public void ShowExplicitData(CoreData coreData)
        {
            CaravanTravelInfo info = _caravanService.GetTravelInfo(coreData);


            _uiService.SetText(DiceCountText, info.DiceSpeed.ToString());

            _uiService.SetText(BonusDistanceText, (info.BonusSpeed > 0 ? "+" + info.BonusSpeed : ""));

            SizeIcon.SetEntityData(EntityTypes.GameplayStat, GameplayStats.MaxSize, info.SizeUsed, info.MaxSize);
            InventoryIcon.SetEntityData(EntityTypes.GameplayStat, GameplayStats.MaxInventory, info.InventoryUsed, info.MaxInventory);

            DailyCurrencyIcons.ShowSmallIdList(EntityTypes.CoreCurrency, info.CurrenciesPerDay.Data, 1);

            TotalCurrencyIcons.ShowSmallIdList(EntityTypes.CoreCurrency, info.CurrenciesPerDay.Data, info.Days);
        }
    }
}

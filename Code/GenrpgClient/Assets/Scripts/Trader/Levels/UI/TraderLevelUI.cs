using OxDb.Client.Doobers.Events;
using OxDb.Client.DynamicUI.Interfaces;
using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Trader.Constants;
using UnityEngine;

namespace OxDb.Client.Trader.Levels.UI
{
    public class TraderLevelUI : BaseBehaviour, IEntityQuantityIcon, IClientEvent
    {
        public GameObject DooberTarget;

        public ProgressBar ExpBar;

        public GText LevelText;
        public override void Init()
        {
            _dispatcher.Dispatch(this);
            _dispatcher.Dispatch(new SetDooberTarget(EntityTypes.CoreCurrency, CoreCurrencyTypes.Exp, ExpBar.FrontBarRHS, true, this));
            base.Init();
            ShowCurrentData();
        }

        public void ShowCurrentData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();

            ExpBar.InitRange(0, coreData.Currencies[CoreCurrencyTypes.Exp], coreData.Vars[TraderVars.ExpToLevelUp]);

            _uiService.SetText(LevelText, coreData.Level.ToString());

        }

        public async Awaitable AnimateToEndOfBar()
        {
            ExpBar.SetValue(ExpBar.GetMaxValue());

            while (ExpBar.IsAnimating())
            {
                await Awaitable.NextFrameAsync();
            }
        }



        public void AddVisualQuantity(long entityTypeId, long entityId, long quantityAdded, bool instant)
        {
            if (entityTypeId != EntityTypes.CoreCurrency || entityId != CoreCurrencyTypes.Exp)
            {
                return;
            }
            ExpBar.AddValue(quantityAdded);
        }
    }
}

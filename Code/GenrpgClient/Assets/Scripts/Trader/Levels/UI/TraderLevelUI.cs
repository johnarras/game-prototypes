using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Interfaces;
using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Constants;
using UnityEngine;

namespace Assets.Scripts.Trader.Levels.UI
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

            ExpBar.InitRange(0, coreData.Vars[TraderVars.ExpToLevelUp], coreData.Currencies[CoreCurrencyTypes.Exp]);

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

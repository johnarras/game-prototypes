using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.CurrencySpend.Entities;
using Genrpg.Shared.Trader.CurrencySpend.WebApi;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.CurrencySpend.MessageHandlers
{
    public class SpendCurrencyResponseHandler : BaseClientWebResponseHandler<SpendCurrencyResponse>
    {
        private IRewardService _rewardService = null;
        protected override async Awaitable InnerProcess(SpendCurrencyResponse response, CancellationToken token)
        {
            if (response.State != ESpendCurrencyCheckState.Success)
            {
                _dispatcher.Dispatch(new ShowFloatingText(StrUtils.SplitOnCapitalLetters(response.State.ToString()), EFloatingTextArt.Error));
            }
            await _rewardService.GiveRewards(_gs.ch, response.Rewards, null);
        }
    }
}

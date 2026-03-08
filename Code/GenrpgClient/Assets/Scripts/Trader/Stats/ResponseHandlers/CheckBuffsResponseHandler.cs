using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;
using Genrpg.Shared.Trader.Stats.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Assets.Scripts.Trader.Stats.ResponseHandlers
{
    public class CheckBuffsResponseHandler : BaseClientWebResponseHandler<CheckBuffsResponse>
    {
        private ITraderStatService _statService = null;
        protected override void InnerProcess(CheckBuffsResponse response, CancellationToken token)
        {
            _statService.UpdateStats(_gs.ch.Get<CoreData>(), _gs.ch.Get<CaravanData>(), _gs.ch.Get<TraderStatData>());

        }
    }
}

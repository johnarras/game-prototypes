using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.Trader.Stats.RewardHelpers
{
    public class TraderDebuffAsyncRewardHelper : IAsyncRewardHelper
    {

        private ITraderStatService _statService = null;
        public long HelperKey => EntityTypes.TraderDebuff;

        public async Task<long> GetQuantityAsync(WebContext context, long entityId)
        {
            return _statService.GetDebuffDays(
                await context.GetAsync<CoreData>(),
                await context.GetAsync<CaravanData>(),
                await context.GetAsync<TraderStatData>(),
                entityId);
        }
        public async Task<bool> GiveRewardAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {

            _statService.AddDebuff(
                await context.GetAsync<CoreData>(),
                await context.GetAsync<CaravanData>(),
                await context.GetAsync<TraderStatData>(),
                entityId,
                quantity);

            return true;

        }
    }
}

using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;
using Genrpg.Shared.Trader.Stats.WebApi;

namespace Genrpg.RequestServer.Trader.Stats.Services
{

    public interface IServerTraderStatService : IInjectable
    {
        Task AddDebuffDaysPlayed(WebContext context, long daysAdded, bool sendResponseToClient);
        Task CheckBuffs(WebContext context, bool isLogin);
    }

    public class ServerTraderStatService : IServerTraderStatService
    {
        private ITraderStatService _statService = null;

        public async Task CheckBuffs(WebContext context, bool isLogin)
        {
            if (isLogin || (context.core.Vars[TraderVars.BuffBits] != 0 && context.core.NextBuffEndsTime <= DateTime.UtcNow))
            {
                _statService.CheckBuffs(context.core, await context.GetAsync<CaravanData>(), await context.GetAsync<TraderStatData>(), isLogin);

                if (!isLogin)
                {
                    context.AddResponse(new CheckBuffsResponse());
                }
            }
        }

        /// <summary>
        /// Add debuff days played, but don't load extended data unless this new number exceeds the
        /// next debuff removal day to reduce load/save
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newDebuffDaysPlayed"></param>
        /// <returns></returns>
        public async Task AddDebuffDaysPlayed(WebContext context, long newDebuffDaysPlayed, bool sendResponseToClient)
        {
            CoreData coreData = await context.GetAsync<CoreData>();

            if (newDebuffDaysPlayed == 0 || coreData.Vars[TraderVars.DebuffBits] == 0)
            {
                return;
            }

            int prevDebuffPlayCount = coreData.Vars[TraderVars.DebuffDaysPlayed];

            int nextDebuffPlayCount = prevDebuffPlayCount + (int)newDebuffDaysPlayed;

            if (sendResponseToClient)
            {
                context.AddResponse(new AddDebuffPlayCountResponse() { DebuffDaysAdded = (int)newDebuffDaysPlayed });
            }
            // If no changes to debuffs, shortcircuit this cheaply.
            if (nextDebuffPlayCount < coreData.Vars[TraderVars.NextDebuffEndsDay])
            {
                coreData.Vars[TraderVars.DebuffDaysPlayed] = nextDebuffPlayCount;
                return;
            }

            _statService.AddDebuffDaysPlayed(coreData, await context.GetAsync<CaravanData>(), await context.GetAsync<TraderStatData>(), newDebuffDaysPlayed);

        }
    }
}

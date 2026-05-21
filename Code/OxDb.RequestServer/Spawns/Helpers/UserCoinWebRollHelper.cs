using OxDb.RequestServer.Core;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Spawns.Entities;

namespace OxDb.RequestServer.Spawns.Helpers
{
    public class UserCoinWebRollHelper : BaseWebRollHelper
    {
        public override long HelperKey => EntityTypes.CoreCurrency;

        public override async Task<long> GetQuantityMult(WebContext context, RollLootArgs rollLootArgs, long entityId)
        {
            await Task.CompletedTask;
            return 1;
        }
    }
}



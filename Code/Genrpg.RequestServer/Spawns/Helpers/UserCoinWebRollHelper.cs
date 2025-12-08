using Genrpg.RequestServer.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spawns.Entities;

namespace Genrpg.RequestServer.Spawns.Helpers
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

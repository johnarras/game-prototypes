using Genrpg.RequestServer.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spawns.Entities;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public class UserCoinWebRollHelper : BaseWebRollHelper
    {
        public override long Key => EntityTypes.CoreCurrency;

        public override async Task<long> GetQuantityMult(WebContext context, RollData rollData, long entityId)
        {
            return 1;
        }
    }
}

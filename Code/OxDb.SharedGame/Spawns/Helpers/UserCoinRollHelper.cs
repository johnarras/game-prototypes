using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Spawns.Entities;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    public class UserCoinRollHelper : BaseRollHelper
    {
        public override long HelperKey => EntityTypes.CoreCurrency;

        public override async ValueTask<long> GetQuantityMult(IUnitDataLookup lookup, RollLootArgs rollLootArgs, long entityId)
        {
            await Task.CompletedTask;
            return 1;
        }
    }
}



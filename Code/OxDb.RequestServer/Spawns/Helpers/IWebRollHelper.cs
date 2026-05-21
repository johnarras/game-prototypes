using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;

using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Settings;

namespace OxDb.RequestServer.Spawns.Helpers
{
    public interface IWebRollHelper : ISetupDictionaryItem<long>
    {
        Task<long> GetQuantityMult(WebContext context, RollLootArgs rollLootArgs, long entityId);
        Task<List<Reward>> Roll<SI>(WebContext context, RollLootArgs rollLootArgs, SI si) where SI : ISpawnItem;
    }
}



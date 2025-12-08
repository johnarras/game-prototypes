using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public interface IWebRollHelper : ISetupDictionaryItem<long>
    {
        Task<long> GetQuantityMult(WebContext context, RollLootArgs rollLootArgs, long entityId);
        Task<List<Reward>> Roll<SI>(WebContext context, RollLootArgs rollLootArgs, SI si) where SI : ISpawnItem;
    }
}

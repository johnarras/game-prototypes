using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public interface IWebRollHelper : ISetupDictionaryItem<long>
    {
        Task<long> GetQuantityMult(WebContext context, RollData rollData, long entityId);
        Task<List<Reward>> Roll<SI>(WebContext context, RollData rollData, SI si) where SI : ISpawnItem;
    }
}

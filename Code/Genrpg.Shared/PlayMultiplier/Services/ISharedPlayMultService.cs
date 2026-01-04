using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.PlayMultiplier.Services
{
    public interface ISharedPlayMultService : IInjectable
    {
        long GetMaxMult(CoreData coreData);

        List<long> GetValidMults(CoreData coreData);
    }
}



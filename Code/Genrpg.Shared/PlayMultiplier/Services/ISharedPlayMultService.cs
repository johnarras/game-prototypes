using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.PlayMultiplier.Services
{
    public interface ISharedPlayMultService : IInjectable
    {
        int GetMaxMult(CoreData coreData);

        List<int> GetValidMults(CoreData coreData);
    }
}



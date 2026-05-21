using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.PlayMultiplier.Services
{
    public interface ISharedPlayMultService : IInjectable
    {
        int GetMaxMult(CoreData coreData);

        List<int> GetValidMults(CoreData coreData);
    }
}



using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Units.Loaders
{
    public interface IUnitDataLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        Task<ITopLevelUnitData> LoadFullData(Unit unit);
        Task<ITopLevelUnitData> LoadTopLevelData(Unit unit);

        IUnitData Create(Unit unit);
        bool IsUserData();
        Type GetServerType();
        bool IsClientOnlyData();
        List<CreateIndexData> GetIndexes();
    }

}



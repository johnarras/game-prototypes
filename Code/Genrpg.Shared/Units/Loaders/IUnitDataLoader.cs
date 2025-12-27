using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Loaders
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



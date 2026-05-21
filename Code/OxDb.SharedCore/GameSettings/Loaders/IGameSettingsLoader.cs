using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedCore.GameSettings.Loaders
{
    /// <summary>
    /// Use for mapping between database and server. Split from mapper so client<->server and server<->database can vary independently
    /// </summary>
    public interface IGameSettingsLoader : ISetupDictionaryItem<Type>, IInitializable
    {
        Type GetChildType();
        Task<List<ITopLevelSettings>> LoadAll(ISearchRepositoryService repoSystem, bool createDefaultIfMissing);
        List<CreateIndexData> GetIndexes();
    }
}



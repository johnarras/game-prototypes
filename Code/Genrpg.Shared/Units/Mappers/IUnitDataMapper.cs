using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Units.Mappers
{
    public interface IUnitDataMapper : ISetupDictionaryItem<Type>
    {
        Version GetMinClientVersion();
        Version GetMaxClientVersion();
        IUnitData MapToAPI(IUnitData serverObject);
        bool SendToClient();
    }
}



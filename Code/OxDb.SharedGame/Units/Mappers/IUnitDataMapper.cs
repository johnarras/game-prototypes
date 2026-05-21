using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System;

namespace OxDb.SharedGame.Units.Mappers
{
    public interface IUnitDataMapper : ISetupDictionaryItem<Type>
    {
        Version GetMinClientVersion();
        Version GetMaxClientVersion();
        IUnitData MapToAPI(IUnitData serverObject);
        bool SendToClient();
    }
}



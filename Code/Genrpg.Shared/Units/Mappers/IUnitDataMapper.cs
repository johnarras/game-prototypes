using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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

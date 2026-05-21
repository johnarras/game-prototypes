using OxDb.SharedCore.DataStores.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Units
{
    public interface ITopLevelUnitData : IUnitData, IVersionedData
    {
        List<IUnitData> GetChildren();
    }
}



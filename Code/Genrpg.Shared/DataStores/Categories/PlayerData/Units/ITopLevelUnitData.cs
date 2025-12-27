using Genrpg.Shared.DataStores.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Units
{
    public interface ITopLevelUnitData : IUnitData, IUpdateData
    {
        List<IUnitData> GetChildren();
    }
}



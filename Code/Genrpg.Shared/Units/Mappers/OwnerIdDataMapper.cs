using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Units.Mappers
{
    public class OwnerIdDataMapper<TParent, TChild, TDto> : OwnerDataMapper<TParent, TChild, TDto>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData, IId
        where TDto : OwnerDtoList<TParent,TChild>
    {
    }
}

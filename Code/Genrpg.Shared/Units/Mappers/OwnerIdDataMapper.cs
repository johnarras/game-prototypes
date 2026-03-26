using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Units.Mappers
{
    public class OwnerIdDataMapper<TParent, TChild, TDto> : OwnerDataMapper<TParent, TChild, TDto>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData, IId
        where TDto : OwnerDtoList<TParent, TChild>
    {
    }
}



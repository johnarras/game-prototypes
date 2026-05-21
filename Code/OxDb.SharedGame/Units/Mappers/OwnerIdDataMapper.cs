using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;

namespace OxDb.SharedGame.Units.Mappers
{
    public class OwnerIdDataMapper<TParent, TChild, TDto> : OwnerDataMapper<TParent, TChild, TDto>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData, IId
        where TDto : OwnerDtoList<TParent, TChild>
    {
    }
}



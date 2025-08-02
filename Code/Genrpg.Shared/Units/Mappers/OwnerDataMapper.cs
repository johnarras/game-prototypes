using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using System;
using System.Linq;

namespace Genrpg.Shared.Units.Mappers
{
    public class OwnerDataMapper<TParent, TChild, TDto> : UnitDataMapper<TParent>
        where TParent : OwnerObjectList<TChild>, new()
        where TChild : OwnerPlayerData, IChildUnitData
        where TDto : OwnerDtoList<TParent, TChild>
    {
        public override IUnitData MapToAPI(IUnitData serverObject)
        {
            TParent parent = serverObject as TParent;

            TDto dto = Activator.CreateInstance<TDto>();

            dto.Parent = parent;
            dto.Children = parent.GetData().ToList();

            return dto;
        }
    }
}

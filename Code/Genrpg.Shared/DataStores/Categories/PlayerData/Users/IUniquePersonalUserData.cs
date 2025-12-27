using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Users
{
    public interface IUniquePersonalUserData : IPartitionedData, IUserData, IUnitData, IStringId
    {
    }
}

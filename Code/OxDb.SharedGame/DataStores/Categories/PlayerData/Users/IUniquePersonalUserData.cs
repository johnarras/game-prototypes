using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.DataStores.Interfaces;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Users
{
    public interface IUniquePersonalUserData : IPartitionedData, IUserData, IUnitData, IStringId
    {
        int GetOffsetBit();
        PersonalDataAccumulation GetAccumulation();
        bool WasEverSaved();
    }
}

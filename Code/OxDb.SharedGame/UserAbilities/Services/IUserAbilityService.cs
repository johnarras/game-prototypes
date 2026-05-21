using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;

namespace OxDb.SharedGame.UserAbilities.Services
{
    public interface IUserAbilityService : IInitializable
    {
        long GetAbilityTotal(IFilteredObject obj, long userAbilityId, long upgradeLevel);
    }
}



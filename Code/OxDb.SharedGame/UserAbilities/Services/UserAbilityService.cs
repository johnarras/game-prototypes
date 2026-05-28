using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.UserAbilities.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.UserAbilities.Services
{
    public class UserAbilityService : IUserAbilityService
    {
        private IGameData _gameData = null;

        public async System.Threading.Tasks.Task Initialize(CancellationToken token)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }

        public long GetAbilityTotal(IFilteredObject filtered, long userAbilityId, long upgradeRank)
        {
            UserAbilityType abilityType = _gameData.Get<UserAbilitySettings>(filtered).Get(userAbilityId);

            if (abilityType == null)
            {
                return upgradeRank;
            }

            return abilityType.BaseQuantity + upgradeRank * abilityType.QuantityPerRank;
        }

    }
}




using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Users.Settings;
using Genrpg.Shared.UserStats.Constants;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreUserLoadUpdater : IUserLoadUpdater
    {
        private IGameData _gameData = null;

        public int Order => 1;

        public Type Key => GetType();


        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            NewUserSettings newUserSettings = _gameData.Get<NewUserSettings>(context.user);

            if (userData.Vars.Get(UserVars.PlayMult) == 0)
            {
                userData.Coins.Add(UserCoinTypes.HardCurrency, newUserSettings.Tokens);   
                userData.Coins.Add(UserCoinTypes.Energy, newUserSettings.Energy);
            }

            RaiseToMinStat(userData, UserVars.PlayMult, BoardGameConstants.MinPlayMult);
            RaiseToMinStat(userData, UserVars.EnergyPerHour, newUserSettings.EnergyPerHour);
            RaiseToMinStat(userData, UserVars.TotalEnergyStorage, newUserSettings.TotalEnergyStorage);
            RaiseToMinStat(userData, UserVars.MarkerId, newUserSettings.MarkerId);
            RaiseToMinStat(userData, UserVars.MarkerTier, newUserSettings.MarkerTier);
        }

        protected void RaiseToMinStat(CoreUserData userData, long userStatId, long minValue)
        {

            if (userData.Vars.Get(userStatId) < minValue)
            {
                userData.Vars.Set(userStatId, (short) minValue);
            }
        }
    }
}

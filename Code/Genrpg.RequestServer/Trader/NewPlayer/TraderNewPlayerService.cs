using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.CoreCurrencies.Settings;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.LevelTracks.Settings;
using Genrpg.Shared.NewPlayers.Settings;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Holdings.PlayerData;
using Genrpg.Shared.Trader.Stats.Settings;

namespace Genrpg.RequestServer.Trader.NewPlayer
{

    public interface ITraderNewPlayerService : IInjectable
    {
        Task UpdatePlayerOnLogin(WebContext context, bool onLogin);
    }

    public class TraderNewPlayerService : ITraderNewPlayerService
    {

        private IGameData _gameData = null;

        public async Task UpdatePlayerOnLogin(WebContext context, bool onLogin)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            List<Reward> newRewards = new List<Reward>();

            CoreCurrencyTypeSettings currencySettings = _gameData.Get<CoreCurrencyTypeSettings>(context.user);

            NewPlayerBonusSettings newPlayerSettings = _gameData.Get<NewPlayerBonusSettings>(context.user);

            List<LevelTrackReward> levelRewards = _gameData.Get<LevelTrackRewardSettings>(context.user).GetData().Where(x => x.Level <= userData.Level).ToList();

            TraderStatSettings statSettings = _gameData.Get<TraderStatSettings>(context.user);

            TraderStatData statData = await context.GetAsync<TraderStatData>();

            HoldingsData holdings = await context.GetAsync<HoldingsData>();

            foreach (IReward rew in newPlayerSettings.GetData())
            {
                if (rew.EntityTypeId == EntityTypes.BaseTraderStat)
                {
                    statData.Stats.Get(rew.EntityId).RaiseBaseToValue(rew.Quantity);
                }
                else if (userData.Level < 1 && rew.EntityTypeId == EntityTypes.CoreCurrency)
                {
                    userData.Currencies.Set(rew.EntityId, rew.Quantity);
                }
                else if (rew.EntityTypeId == EntityTypes.Animal)
                {
                    holdings.AnimalsOwned.SetBit(rew.EntityId);
                }
            }

            foreach (IReward rew in levelRewards)
            {
                if (rew.EntityTypeId == EntityTypes.BaseTraderStat)
                {
                    statData.Stats.Get(rew.EntityId).RaiseBaseToValue(rew.Quantity);
                }
                else if (rew.EntityTypeId == EntityTypes.Animal)
                {
                    holdings.AnimalsOwned.SetBit(rew.EntityId);
                }
            }
            if (context.user.Level < 1)
            {
                context.user.Level = 1;
                context.user.Exp = 0;
                context.user.SetNextHourlyUpdate();
            }
        }
    }
}

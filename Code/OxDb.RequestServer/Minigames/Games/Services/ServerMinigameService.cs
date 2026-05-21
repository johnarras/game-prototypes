using OxDb.RequestServer.Core;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Minigames.Games.Settings;
using OxDb.SharedGame.Minigames.Games.WebApi;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;

namespace OxDb.RequestServer.Minigames.Games.Services
{
    public interface IServerMingiameService : IInjectable
    {
        Task EndMinigame(WebContext context, long minigameTypeId, bool wonGame);
    }
    public class ServerMinigameService : IServerMingiameService
    {
        private IGameData _gameData = null;
        private IRewardService _rewardService = null;

        public async Task EndMinigame(WebContext context, long minigameTypeId, bool wonGame)
        {

            EndMinigameResponse response = new EndMinigameResponse();

            CoreData coreData = await context.GetAsync<CoreData>();

            MinigameType mtype = _gameData.Get<MinigameTypeSettings>(coreData).Get(minigameTypeId);

            if (mtype == null)
            {
                response.ErrorMessage = "No such minigame";
                context.AddResponse(response);
                return;
            }

            long coins = (wonGame ? mtype.WinCoins : mtype.LoseCoins);

            if (coins != 0)
            {
                RewardData rewardData = new RewardData();
                Reward rew = new Reward()
                {
                    EntityTypeId = EntityTypes.CoreCurrency,
                    EntityId = CoreCurrencyTypes.Coins,
                    Quantity = coins,
                };

                rewardData.Rewards.Add(_rewardService.CreateRewardList(RewardSources.Minigame, new List<Reward>() { rew }, minigameTypeId));

                response.Rewards = rewardData;

                await _rewardService.GiveRewards(context, rewardData.Rewards[0].Rewards, rewardData.Rewards[0].RewardSourceId, new RewardParams());
            }

            response.Success = true;
            response.MinigameTypeId = minigameTypeId;
            context.AddResponse(response);

            await Task.CompletedTask;
        }
    }
}

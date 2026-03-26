using Genrpg.RequestServer.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Minigames.Games.Settings;
using Genrpg.Shared.Minigames.Games.WebApi;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;

namespace Genrpg.RequestServer.Minigames.Games.Services
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

                rewardData.Rewards.Add(new RewardList() { Rewards = new List<Reward> { rew } });

                response.Rewards = rewardData;

                await _rewardService.GiveRewards(context, rewardData.Rewards[0].Rewards, new RewardParams());
            }

            response.Success = true;
            response.MinigameTypeId = minigameTypeId;
            context.AddResponse(response);

            await Task.CompletedTask;
        }
    }
}

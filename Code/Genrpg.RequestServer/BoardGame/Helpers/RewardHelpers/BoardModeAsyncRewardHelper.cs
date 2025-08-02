using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.BoardGame.Helpers.RewardHelpers
{
    public class BoardModeAsyncRewardHelper : IAsyncRewardHelper
    {
        public long Key => EntityTypes.BoardMode;
   
        public async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            await Task.CompletedTask;
        }
    }
}

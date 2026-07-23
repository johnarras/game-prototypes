using OxDb.Client.DynamicUI.Services;
using OxDb.Client.FloatingText.ClientEvents;
using OxDb.Client.Login.Messages.Core;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Minigames.Games.WebApi;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Minigames.Games.ResponseHandlers
{
    public class EndMinigameResponseHandler : BaseClientWebResponseHandler<EndMinigameResponse>
    {
        private IDynamicUIService _dynamicUIService = null;
        protected override async ValueTask InnerProcess(EndMinigameResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
            }
            if (response.Rewards != null)
            {
                foreach (RewardList rlist in response.Rewards.Rewards)
                {
                    foreach (Reward rew in rlist.Rewards)
                    {
                        _dynamicUIService.ShowDefaultEntityDoober(rew.EntityTypeId, rew.EntityId, rew.Quantity);
                    }
                }
            }
            await Task.CompletedTask;
        }
    }
}

using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Core;
using Assets.Scripts.FloatingText.ClientEvents;
using Genrpg.Shared.Minigames.Games.WebApi;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Minigames.Games.ResponseHandlers
{
    public class EndMinigameResponseHandler : BaseClientWebResponseHandler<EndMinigameResponse>
    {
        private IDynamicUIService _dynamicUIService = null;
        protected override async Awaitable InnerProcess(EndMinigameResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage,EFloatingTextArt.Error));
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
        }
    }
}

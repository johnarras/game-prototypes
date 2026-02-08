using Assets.Scripts.Doobers.Events;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Minigames.Games.WebApi;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Assets.Scripts.Minigames.Games.ResponseHandlers
{
    public class EndMinigameResponseHandler : BaseClientWebResponseHandler<EndMinigameResponse>
    {
        protected override void InnerProcess(EndMinigameResponse response, CancellationToken token)
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
                        _dispatcher.Dispatch(new ShowDooberEvent() { EntityTypeId = rew.EntityTypeId, EntityId = rew.EntityId, Quantity = rew.Quantity, StartsInUI = true });
                    }
                }
            }
        }
    }
}

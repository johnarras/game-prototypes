using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Achievements.PlayerData;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Achievements
{
    public class OnUpdateAchievementHandler : BaseClientMapMessageHandler<OnUpdateAchievement>
    {
        protected override async Awaitable InnerProcess(OnUpdateAchievement msg, CancellationToken token)
        {
            _gs.ch.Get<AchievementData>().Data[msg.AchievementTypeId] = msg.Quantity;
        }
    }
}



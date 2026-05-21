using OxDb.SharedGame.Achievements.Messages;
using OxDb.SharedGame.Achievements.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Achievements
{
    public class OnUpdateAchievementHandler : BaseClientMapMessageHandler<OnUpdateAchievement>
    {
        protected override async Awaitable InnerProcess(OnUpdateAchievement msg, CancellationToken token)
        {
            _gs.ch.Get<AchievementData>().Data[msg.AchievementTypeId] = msg.Quantity;
            await Task.CompletedTask;
        }
    }
}



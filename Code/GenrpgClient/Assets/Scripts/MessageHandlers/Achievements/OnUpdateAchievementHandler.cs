using OxDb.SharedGame.Achievements.Messages;
using OxDb.SharedGame.Achievements.PlayerData;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.Achievements
{
    public class OnUpdateAchievementHandler : BaseClientMapMessageHandler<OnUpdateAchievement>
    {
        protected override async ValueTask InnerProcess(OnUpdateAchievement msg, CancellationToken token)
        {
            _gs.ch.Get<AchievementData>().Data[msg.AchievementTypeId] = msg.Quantity;
            await Task.CompletedTask;
        }
    }
}



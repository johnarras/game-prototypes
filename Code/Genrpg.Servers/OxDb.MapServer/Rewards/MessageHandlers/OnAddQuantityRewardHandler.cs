using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Rewards.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Rewards.MessageHandlers
{
    public class OnAddQuantityRewardHandler : BaseMapObjectServerMapMessageHandler<OnAddQuantityReward>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnAddQuantityReward message)
        {
            obj.AddMessage(message);
        }
    }
}



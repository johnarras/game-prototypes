using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Rewards.MessageHandlers
{
    public class OnAddQuantityRewardHandler : BaseMapObjectServerMapMessageHandler<OnAddQuantityReward>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnAddQuantityReward message)
        {
            obj.AddMessage(message);
        }
    }
}



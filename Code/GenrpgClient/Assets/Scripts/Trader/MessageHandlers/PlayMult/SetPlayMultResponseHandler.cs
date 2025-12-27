using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.PlayMultiplier.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.PlayMult
{
    public class SetPlayMultResponseHandler : BaseClientWebResponseHandler<SetPlayMultResponse>
    {
        protected override void InnerProcess(SetPlayMultResponse response, CancellationToken token)
        {
            CoreUserData coreData = _gs.ch.Get<CoreUserData>();
            coreData.Mult = response.NewPlayMult;
        }
    }
}



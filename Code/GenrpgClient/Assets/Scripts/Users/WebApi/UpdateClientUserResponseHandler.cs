using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Users.WebApi;
using System.Threading;

namespace Assets.Scripts.Users.WebApi
{
    public class UpdateClientUserResponseHandler : BaseClientWebResponseHandler<UpdateClientUserResponse>
    {
        protected override void InnerProcess(UpdateClientUserResponse result, CancellationToken token)
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            coreData.Level = result.Level;
        }
    }
}



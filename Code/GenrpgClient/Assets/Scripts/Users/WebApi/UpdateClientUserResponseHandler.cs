using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Users.WebApi;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Users.WebApi
{
    public class UpdateClientUserResponseHandler : BaseClientWebResponseHandler<UpdateClientUserResponse>
    {
        protected override async ValueTask InnerProcess(UpdateClientUserResponse result, CancellationToken token)
        {
            await Task.CompletedTask;
            CoreData coreData = _gs.ch.Get<CoreData>();
            coreData.Level = result.Level;
        }
    }
}



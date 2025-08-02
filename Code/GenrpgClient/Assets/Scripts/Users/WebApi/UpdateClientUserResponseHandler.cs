using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Users.WebApi;
using System.Threading;

namespace Assets.Scripts.Users.WebApi
{
    public class UpdateClientUserResponseHandler : BaseClientWebResponseHandler<UpdateClientUserResponse>
    {
        protected override void InnerProcess(UpdateClientUserResponse result, CancellationToken token)
        {
            _gs.user.Level = result.Level;
        }
    }
}

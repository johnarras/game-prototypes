using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Users.WebApi;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Users.WebApi
{
    public class UpdateClientUserResponseHandler : BaseClientWebResponseHandler<UpdateClientUserResponse>
    {
        protected override async Awaitable InnerProcess(UpdateClientUserResponse result, CancellationToken token)
        {
            await Task.CompletedTask;
            CoreData coreData = _gs.ch.Get<CoreData>();
            coreData.Level = result.Level;
        }
    }
}



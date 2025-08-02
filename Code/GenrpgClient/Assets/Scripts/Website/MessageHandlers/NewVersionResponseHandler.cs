using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Accounts.WebApi.NewVersions;
using Genrpg.Shared.UI.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Website.MessageHandlers
{
    public class NewVersionResponseHandler : BaseClientWebResponseHandler<NewVersionResponse>
    {
        protected override void InnerProcess(NewVersionResponse response, CancellationToken token)
        {
            _dispatcher.Dispatch(response);
        }
    }
}

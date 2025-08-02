using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers
{
    public interface IGameAuthRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Execute(WebContext context, IGameAuthRequest request, CancellationToken token);
    }
}

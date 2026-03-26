using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.RequestServer.ClientUserRequests.RequestHandlers
{
    public interface IGameClientRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, IWebRequest request, CancellationToken token);
    }
}



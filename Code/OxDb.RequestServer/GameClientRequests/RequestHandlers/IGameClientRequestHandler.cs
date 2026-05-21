using OxDb.RequestServer.Core;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Website.Requests.Interfaces;

namespace OxDb.RequestServer.ClientUserRequests.RequestHandlers
{
    public interface IGameClientRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, IWebRequest request, CancellationToken token);
    }
}



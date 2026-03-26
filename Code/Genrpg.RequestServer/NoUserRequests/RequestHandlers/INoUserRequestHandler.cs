using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.RequestServer.NoUserRequests.RequestHandlers
{
    public interface INoUserRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, INoUserRequest request, CancellationToken token);
    }
}



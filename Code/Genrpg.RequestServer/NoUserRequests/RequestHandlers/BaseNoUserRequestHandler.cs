
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.NoUserRequests.RequestHandlers
{
    public abstract class BaseNoUserRequestHandler<TRequest> : INoUserRequestHandler where TRequest : INoUserRequest
    {

        protected abstract Task HandleRequestInternal(WebContext context, TRequest request, CancellationToken token);

        public Type Key => typeof(TRequest);

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, INoUserRequest request, CancellationToken token)
        {
            await HandleRequestInternal(context, (TRequest)request, token);
        }

        protected void ShowError(WebContext context, string msg)
        {
            context.Responses.AddResponse(new ErrorResponse() { Error = msg });
        }
    }

}

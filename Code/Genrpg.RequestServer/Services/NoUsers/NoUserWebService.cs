
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;

namespace Genrpg.RequestServer.Services.NoUsers
{
    public class NoUserWebService : INoUserWebService
    {
        private ILogService _logService = null;
        private IWebServerService _loginServerService = null;
        private ITextSerializer _serializer = null;

        public async Task HandleNoUserRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {

            try
            {
                foreach (INoUserRequest comm in requestSet.Requests)
                {
                    INoUserRequestHandler handler = _loginServerService.GetNoUserCommandHandler(comm.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<IWebResponse> errors = new List<IWebResponse>();

                foreach (IWebResponse response in context.GetResponseList())
                {
                    if (response is ErrorResponse error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.ClearResponses();
                    context.AddResponseRange(errors);
                    return;
                }

            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + requestSet.Requests.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                context.ShowError(errorMessage);
            }

            return;
        }
    }
}



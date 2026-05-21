using OxDb.ServerCore.Core;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.PlatformServer.Entities
{

    public class AuthWebContext : ServerGameState, IWebContext
    {
        protected WebResponseList Responses { get; set; } = new WebResponseList();

        public List<IWebResponse> GetResponseList()
        {
            return Responses.GetResponses();
        }

        public void AddResponse(IWebResponse response)
        {
            Responses.AddResponse(response);
        }

        public void AddFront(IWebResponse response)
        {
            Responses.AddFront(response);
        }

        public void ClearResponses()
        {
            Responses.Clear();
        }

        public void AddResponseRange(List<IWebResponse> responses)
        {
            Responses.AddRange(responses);
        }


        public void ShowError(string error)
        {
            AddResponse(new ErrorResponse() { Error = error });
        }

        public void Dispose()
        {

        }
    }
}



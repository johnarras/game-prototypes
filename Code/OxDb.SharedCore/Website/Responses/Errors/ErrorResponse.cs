using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedCore.Website.Responses.Errors
{
    public class ErrorResponse : IWebResponse, IClientEvent
    {
        public string Error { get; set; }
    }
}

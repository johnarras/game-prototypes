using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.Error
{
    public class ErrorResponse : IWebResponse
    {
        public string Error { get; set; }
    }
}



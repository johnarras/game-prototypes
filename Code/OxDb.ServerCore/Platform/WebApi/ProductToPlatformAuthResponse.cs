using OxDb.ServerCore.Platform.Constants;
using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.ServerCore.Platform.WebApi
{
    public class ProductToPlatformAuthResponse : IWebResponse
    {
        public EPlatformAuthStates State { get; set; }

    }
}

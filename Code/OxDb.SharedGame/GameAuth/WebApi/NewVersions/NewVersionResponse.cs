using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedGame.GameAuth.WebApi.NewVersions
{
    public class NewVersionResponse : IWebResponse
    {
        public string MinNewClientVersion { get; set; }
    }
}



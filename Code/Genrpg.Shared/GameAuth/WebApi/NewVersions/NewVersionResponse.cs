using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameAuth.WebApi.NewVersions
{
    public class NewVersionResponse : IWebResponse
    {
        public string MinNewClientVersion { get; set; }
    }
}



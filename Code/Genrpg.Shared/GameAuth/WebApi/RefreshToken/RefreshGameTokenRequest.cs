using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameAuth.WebApi.RefreshToken
{
    public class RefreshGameTokenRequest : IWebRequest
    {
        public string GameUserId { get; set; }
        public string RefreshToken { get; set; }
    }
}

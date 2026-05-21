using OxDb.SharedCore.Website.Requests.Interfaces;

namespace OxDb.SharedGame.GameAuth.WebApi.RefreshToken
{
    public class RefreshGameTokenRequest : IWebRequest
    {
        public string GameUserId { get; set; }
        public string RefreshToken { get; set; }
    }
}

using Genrpg.Shared.GameAuth.Interfaces;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameAuth.WebApi.RefreshToken
{
    public class RefreshGameTokenResponse : IWebResponse, IGameSessionState
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string GameUserId { get; set; }
        public string RefreshToken { get; set; }
        public string SessionToken { get; set; }
    }
}

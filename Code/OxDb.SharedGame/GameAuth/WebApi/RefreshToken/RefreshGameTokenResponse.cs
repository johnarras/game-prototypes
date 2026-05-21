using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.GameAuth.Interfaces;

namespace OxDb.SharedGame.GameAuth.WebApi.RefreshToken
{
    public class RefreshGameTokenResponse : IWebResponse, IGameSessionState
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string GameUserId { get; set; }
        public string RefreshToken { get; set; }
        public string SelfContainedToken { get; set; }
        public string SessionId { get; set; }
        public string ServerName { get; set; }
        public string ServerVersion { get; set; }
        public string ServerEnv { get; set; }
    }
}

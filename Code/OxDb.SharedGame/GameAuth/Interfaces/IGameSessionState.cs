namespace OxDb.SharedGame.GameAuth.Interfaces
{
    public interface IGameSessionState
    {
        public string SelfContainedToken { get; set; }
        public string RefreshToken { get; set; }
        public string SessionId { get; set; }
        public string ServerName { get; set; }
        public string ServerVersion { get; set; }
        public string ServerEnv { get; set; }

    }
}

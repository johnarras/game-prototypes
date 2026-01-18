namespace Genrpg.Shared.GameAuth.Interfaces
{
    public interface IGameSessionState
    {
        public string SessionToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.Minigames.Games.WebApi
{
    public class EndMinigameRequest : IClientUserRequest
    {
        public long MinigameTypeId { get; set; }
        public bool WonGame { get; set; }
    }
}

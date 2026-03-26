using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Minigames.Games.WebApi
{
    public class EndMinigameRequest : IClientUserRequest
    {
        public long MinigameTypeId { get; set; }
        public bool WonGame { get; set; }
    }
}

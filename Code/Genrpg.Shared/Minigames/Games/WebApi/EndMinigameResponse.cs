using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Minigames.Games.WebApi
{
    public class EndMinigameResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long MinigameTypeId { get; set; }
        public RewardData Rewards { get; set; }
    }
}

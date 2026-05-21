using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Website.Responses.Interfaces;


namespace OxDb.SharedGame.Minigames.Games.WebApi
{
    public class EndMinigameResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long MinigameTypeId { get; set; }
        public RewardData Rewards { get; set; }
    }
}

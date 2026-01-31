using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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

using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Minigames.Games.WebApi
{
    public class EndMinigameRequest : IClientUserRequest
    {
        public long MinigameTypeId { get; set; }
        public bool WonGame { get; set; }
    }
}

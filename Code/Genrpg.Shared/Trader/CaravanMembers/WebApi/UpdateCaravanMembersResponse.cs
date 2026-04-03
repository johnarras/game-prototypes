using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.CaravanMembers.WebApi
{
    public class UpdateCaravanMembersResponse : IWebResponse
    {
        public List<CurrentCaravanMember> CurrentMembers { get; set; } = new List<CurrentCaravanMember>();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}



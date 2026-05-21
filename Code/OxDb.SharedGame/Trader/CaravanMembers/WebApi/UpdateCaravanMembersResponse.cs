using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Trader.Caravans.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.CaravanMembers.WebApi
{
    public class UpdateCaravanMembersResponse : IWebResponse
    {
        public List<CurrentCaravanMember> CurrentMembers { get; set; } = new List<CurrentCaravanMember>();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}



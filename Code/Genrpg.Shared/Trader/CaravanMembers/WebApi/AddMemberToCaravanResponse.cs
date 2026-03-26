using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.CaravanMembers.WebApi
{
    public class AddMemberToCaravanResult : IWebResponse
    {
        public long CaravanMemberId { get; set; }
        public long SkinTypeId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public CaravanTravelInfo Travel { get; set; }
    }
}



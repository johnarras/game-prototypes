using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.CaravanMembers.WebApi
{
    public class AddCaravanMemberToCaravanRequest : IClientUserRequest
    {
        public long CaravanMemberId { get; set; }
    }
}

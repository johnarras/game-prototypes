using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    public class AccountLoginRequest : IAccountAuthRequest
    {
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public long ProductId { get; set; }

        public string ReferrerId { get; set; }

        public string DeviceId { get; set; }


    }
}



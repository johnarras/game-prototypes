using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedPlatform.Accounts.WebApi.Login
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



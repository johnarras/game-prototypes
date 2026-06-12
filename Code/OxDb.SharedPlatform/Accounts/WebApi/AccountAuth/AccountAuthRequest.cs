using OxDb.SharedPlatform.Accounts.Constants;

namespace OxDb.SharedPlatform.Accounts.WebApi.AccountAuth
{
    public class AccountAuthRequest : IAccountAuthRequest
    {
        public EAuthTypes AuthType { get; set; }
        public string AccountId { get; set; }
        public string UserIdentity { get; set; }
        public string UserSecret { get; set; }
        public string ReferrerId { get; set; }
        public long ProductId { get; set; }
        public string ClientVersion { get; set; }

        public string DeviceId { get; set; }

        public string InstallSource { get; set; }
    }
}

using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedPlatform.Accounts.Constants;

namespace OxDb.SharedPlatform.Accounts.WebApi.AccountAuth
{
    public class AccountAuthResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public EAuthTypes LastAuthType { get; set; }
        public EAuthTypes ValidAuthTypes { get; set; }
        public string AccountId { get; set; }
        public string LoginToken { get; set; }
        public string DeviceId { get; set; }
        public string AccountSessionId { get; set; }
        public string ProductUserId { get; set; }
        public long DataBits { get; set; }
        public long ProductId { get; set; }
        public string DisplayName { get; set; }
        public string InstallSource { get; set; }
        public string OneTimeGuestAccountId { get; set; }
        public string OneTimeGuestSecret { get; set; }

        public AccountAuthResponse()
        {
        }

        public AccountAuthResponse(string errorMessage)
        {
            ErrorMessage = errorMessage; 
        }
    }
}

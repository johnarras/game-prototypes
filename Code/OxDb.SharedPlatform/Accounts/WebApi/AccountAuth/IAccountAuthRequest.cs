using OxDb.SharedCore.Website.Requests.Interfaces;
using OxDb.SharedPlatform.Accounts.Constants;

namespace OxDb.SharedPlatform.Accounts.WebApi.AccountAuth
{
    public interface IAccountAuthRequest : IWebRequest
    {
        EAuthTypes AuthType { get; set; }
        string AccountId { get; set; }
        string UserIdentity { get; set; }
        string UserSecret { get; set; }
        long ProductId { get; set; }
        string ClientVersion { get; set; }
        string DeviceId { get; set; }
        string InstallSource { get; set; }
        string ReferrerId { get; set; }
    }
}

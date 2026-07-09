using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.SharedPlatform.Accounts.Constants;

namespace OxDb.PlatformServer.Accounts.Entities
{
    public class AccountAuthResult
    {

        private EAuthTypes _authType;
        public EAuthTypes AuthType => _authType;

        public bool Success { get; set; }

        public string ErrorMessage { get; set; } = null!;

        public Account CurrentAccount { get; set; } = null!;

        public string OneTimeGuestAccountId { get; set; } = null!;

        public string OneTimeGuestSecret { get; set; } = null!;

        

        public AccountAuthResult(EAuthTypes authType)
        {
            _authType = authType;
        }
    }
}

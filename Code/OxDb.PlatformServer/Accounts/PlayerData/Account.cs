using OxDb.SharedPlatform.Accounts.Constants;

namespace OxDb.PlatformServer.Accounts.PlayerData
{

    public class Account : BaseAccountData
    {
        public override string Id { get; set; }
        public string DisplayName { get; set; }
        public string LowerDisplayName { get; set; }

        public string ReferrerAccountId { get; set; }

        public string Email { get; set; }
        public string LowerEmail { get; set; }
        public string EmailPasswordHash { get; set; }
        public string EmailPasswordSalt { get; set; }

        public string GooglePlayUserId { get; set; }
        public string GooglePlayEmail { get; set; }
        public string LowerGoogleEmail { get; set; }

        public string AppleUserId { get; set; }
        public string AppleEmail { get; set; }
        public string LowerAppleEmail { get; set; }

        public string FacebookUserId { get; set; }
        public string FacebookEmail { get; set; }
        public string LowerFacebookEmail { get; set; }

        public string GuestDeviceId { get; set; }
        public string GuestSecretHash { get; set; }
        public string GuestSecretSalt { get; set; }

        public DateTime CreatedOn { get; set; }
        public long OriginalProductId { get; set; }
        public int Flags { get; set; }

        public string InstallSource { get; set; }

        public List<DeviceAuthStatus> AuthRecords { get; set; } = new List<DeviceAuthStatus>();

        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public List<ProductRecord> Products { get; set; } = new List<ProductRecord>();

        public Account()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public EAuthTypes GetValidAuthTypes()
        {
            EAuthTypes allAuthTypes = EAuthTypes.None;
            if (!string.IsNullOrEmpty(Email) && 
                !string.IsNullOrEmpty(EmailPasswordSalt) && 
                !string.IsNullOrEmpty(EmailPasswordHash))
            {
                allAuthTypes |= EAuthTypes.Email;
            }

            if (!string.IsNullOrEmpty(GooglePlayUserId))
            {
                allAuthTypes |= EAuthTypes.GooglePlay;
            }

            if (!string.IsNullOrEmpty(AppleUserId))
            {
                allAuthTypes |= EAuthTypes.iOS;
            }

            if (!string.IsNullOrEmpty(FacebookUserId))
            {
                allAuthTypes |= EAuthTypes.Facebook;    
            }

            if (!string.IsNullOrEmpty(GuestDeviceId) && 
                !string.IsNullOrEmpty(GuestSecretSalt) && 
                !string.IsNullOrEmpty(GuestSecretHash))
            {
                allAuthTypes |= EAuthTypes.Guest;   
            }

            return allAuthTypes;
        }
    }

    public class DeviceAuthStatus
    {
        public string TokenHash { get; set; }
        public string TokenSalt { get; set; }
        public string DeviceId { get; set; }
    }

    public class ProductRecord
    {
        public long ProductId { get; set; }
        public string ProductUserId { get; set; }
        public long DataBits { get; set; }

        public string InstallSource { get; set; }
    }

}



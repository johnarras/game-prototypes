namespace OxDb.PlatformServer.Accounts.PlayerData
{

    public class Account : BaseAccountData
    {
        public override string Id { get; set; }
        public string DisplayName { get; set; }
        public string LowerDisplayName { get; set; }

        public string ReferrerAccountId { get; set; }
        public string Email { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }

        public string GuestToken { get; set; }

        public string GoogleUserId { get; set; }
        public string GoogleEmail { get; set; }

        public string AppleUserId { get; set; }
        public string AppleEmail { get; set; }

        public string LoginTokenHash { get; set; }
        public string LoginTokenSalt { get; set; }

        public string LowerEmail { get; set; }
        public DateTime CreatedOn { get; set; }
        public long OriginalProductId { get; set; }
        public int Flags { get; set; }

        public string InstallSource { get; set; }

        public List<AuthRecord> AuthRecords { get; set; } = new List<AuthRecord>();

        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public List<ProductRecord> Products { get; set; } = new List<ProductRecord>();

        public Account()
        {
            CreatedOn = DateTime.UtcNow;
        }
    }

    public class AuthRecord
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



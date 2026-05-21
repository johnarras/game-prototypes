namespace OxDb.PlatformServer.Accounts.PlayerData
{

    public class Account : BaseAccountData
    {
        public override string Id { get; set; }
        public string ShareId { get; set; }
        public string LowerShareId { get; set; }
        public string ReferrerAccountId { get; set; }
        public string Name { get; set; }
        public string LowerName { get; set; }
        public string Email { get; set; }
        public string LowerEmail { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public List<AuthRecord> AuthRecords { get; set; } = new List<AuthRecord>();
        public DateTime CreatedOn { get; set; }
        public long OriginalProductId { get; set; }
        public int Flags { get; set; }
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
    }

}



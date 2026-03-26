namespace Genrpg.Shared.Accounts.PlayerData
{
    public class AccountIdIncrement : BaseAccountData
    {
        public const string DocId = "Default";

        public override string Id { get; set; }
        public long AccountId { get; set; } = 0;
    }
}



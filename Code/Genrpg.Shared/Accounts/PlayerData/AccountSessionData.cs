using MessagePack;

namespace Genrpg.Shared.Accounts.PlayerData
{
    public class AccountSessionData : BaseAccountData
    {
        public override string Id { get; set; }
        public string SessionId { get; set; }
        public string ShareId { get; set; }
    }
}



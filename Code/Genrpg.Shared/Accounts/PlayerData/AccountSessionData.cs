using MessagePack;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [MessagePackObject]
    public class AccountSessionData : BaseAccountData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string SessionId { get; set; }
        [Key(2)] public string ShareId { get; set; }
    }
}

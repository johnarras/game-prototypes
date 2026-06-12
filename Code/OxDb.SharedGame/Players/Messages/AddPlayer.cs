using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Players.Messages
{
    [MessagePackObject]
    public sealed class AddPlayer : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string GameUserId { get; set; }
        [Key(1)] public string CharacterId { get; set; }
        [Key(2)] public string FullToken { get; set; }
    }
}



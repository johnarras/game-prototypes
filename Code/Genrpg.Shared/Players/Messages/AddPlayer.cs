using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Players.Messages
{
    [MessagePackObject]
    public sealed class AddPlayer : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string GameUserId { get; set; }
        [Key(1)] public string CharacterId { get; set; }
        [Key(2)] public string SessionToken { get; set; }
    }
}



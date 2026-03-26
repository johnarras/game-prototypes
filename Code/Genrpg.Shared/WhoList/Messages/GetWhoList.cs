using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.WhoList.Messages
{
    [MessagePackObject]
    public sealed class GetWhoList : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string Args { get; set; }
    }
}



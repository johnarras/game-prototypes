using Genrpg.Shared.MapMessages;
using Genrpg.Shared.WhoList.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.WhoList.Messages
{
    [MessagePackObject]
    public sealed class OnGetWhoList : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public List<WhoListItem> Items { get; set; } = new List<WhoListItem>();

    }
}



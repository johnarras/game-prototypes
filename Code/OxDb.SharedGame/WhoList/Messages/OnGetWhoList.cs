using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.WhoList.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.WhoList.Messages
{
    [MessagePackObject]
    public sealed class OnGetWhoList : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public List<WhoListItem> Items { get; set; } = new List<WhoListItem>();

    }
}



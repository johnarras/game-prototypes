using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.MapObjects.MapObjectAddons.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class OnGetMapObjectStatus : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();
    }
}



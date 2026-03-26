using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnRemoveActionBarItem : BaseInfrequenMapApiMessage
    {
        [Key(0)] public int Index { get; set; }
    }
}



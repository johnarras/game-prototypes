using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class RemoveActionBarItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public int Index { get; set; }
    }
}



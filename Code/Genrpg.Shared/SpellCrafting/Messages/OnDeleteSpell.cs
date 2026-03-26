using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnDeleteSpell : BaseInfrequenMapApiMessage
    {
        [Key(0)] public long SpellId { get; set; }
    }
}



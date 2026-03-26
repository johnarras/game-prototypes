using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class DeleteSpell : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long SpellId { get; set; }
    }
}



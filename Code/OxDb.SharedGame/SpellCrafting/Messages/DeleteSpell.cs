using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class DeleteSpell : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long SpellId { get; set; }
    }
}



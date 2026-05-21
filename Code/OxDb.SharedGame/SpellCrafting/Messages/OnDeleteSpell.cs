using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class OnDeleteSpell : BaseInfrequenMapApiMessage
    {
        [Key(0)] public long SpellId { get; set; }
    }
}



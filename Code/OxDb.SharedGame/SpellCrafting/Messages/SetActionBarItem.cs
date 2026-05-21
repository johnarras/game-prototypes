using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.SpellCrafting.Messages
{
    [MessagePackObject]
    public sealed class SetActionBarItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long SpellId { get; set; }
        [Key(1)] public int Index { get; set; }
    }
}



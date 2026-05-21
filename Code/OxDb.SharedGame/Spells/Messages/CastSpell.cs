using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Spells.Messages
{
    [MessagePackObject]
    public sealed class CastSpell : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long SpellId { get; set; }
        [Key(1)] public string TargetId { get; set; }
    }
}



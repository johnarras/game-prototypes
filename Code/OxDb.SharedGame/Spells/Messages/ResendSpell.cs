using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Spells.Messages
{
    public sealed class ResendSpell : BaseMapMessage
    {
        public string TargetId { get; set; }
        public long ShotsLeft { get; set; }
        public SendSpell SpellMessage { get; set; }
    }
}



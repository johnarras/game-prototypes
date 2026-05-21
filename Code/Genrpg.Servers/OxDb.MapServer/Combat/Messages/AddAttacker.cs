using OxDb.SharedGame.MapMessages;

namespace OxDb.MapServer.Combat.Messages
{
    public sealed class AddAttacker : BaseMapMessage
    {
        public string AttackerId { get; set; }
    }
}


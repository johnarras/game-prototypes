using Genrpg.Shared.MapMessages;

namespace Genrpg.MapServer.Combat.Messages
{
    public sealed class AddAttacker : BaseMapMessage
    {
        public string AttackerId { get; set; }
    }
}


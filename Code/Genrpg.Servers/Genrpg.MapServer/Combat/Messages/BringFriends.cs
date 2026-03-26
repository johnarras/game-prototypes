using Genrpg.Shared.MapMessages;

namespace Genrpg.MapServer.Combat.Messages
{
    public sealed class BringFriends : BaseMapMessage
    {
        public string BringerId { get; set; }
        public string TargetId { get; set; }
        public long BringerFactionId { get; set; }
        public long TargetFactionId { get; set; }
    }
}



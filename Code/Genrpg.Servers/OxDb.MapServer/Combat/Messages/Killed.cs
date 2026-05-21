using OxDb.SharedGame.MapMessages;

namespace OxDb.MapServer.Combat.Messages
{
    public sealed class Killed : BaseMapMessage
    {
        public string UnitId { get; set; }
        public long UnitTypeId { get; set; }
        public long FactionTypeId { get; set; }
        public string ObjId { get; set; }
        public long Level { get; set; }
        public long ZoneId { get; set; }
    }
}


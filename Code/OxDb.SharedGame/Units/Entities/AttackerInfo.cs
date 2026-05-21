using MessagePack;

namespace OxDb.SharedGame.Units.Entities
{
    [MessagePackObject]
    public class AttackerInfo
    {
        [Key(0)] public string AttackerId { get; set; }
        [Key(1)] public string GroupId { get; set; }
    }
}



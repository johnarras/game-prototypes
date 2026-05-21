using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Interactions.Messages
{
    [MessagePackObject]
    public sealed class InteractCommand : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public bool IsSkillLoot { get; set; }
    }
}



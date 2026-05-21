using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Achievements.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateAchievement : BaseMapApiMessage
    {
        [Key(0)] public long AchievementTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
    }
}



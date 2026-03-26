using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Achievements.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateAchievement : BaseMapApiMessage
    {
        [Key(0)] public long AchievementTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
    }
}



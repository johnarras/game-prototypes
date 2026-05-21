using MessagePack;
using OxDb.SharedGame.Spells.Interfaces;

namespace OxDb.SharedGame.Spells.Settings.Effects
{
    [MessagePackObject]
    public class DisplayEffect : IDisplayEffect
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public long EntityTypeId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public float MaxDuration { get; set; }
        [Key(5)] public float DurationLeft { get; set; }

        public bool MatchesOther(IDisplayEffect other)
        {
            return false;
        }
    }
}



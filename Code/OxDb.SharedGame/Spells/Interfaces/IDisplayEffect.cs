using OxDb.SharedCore.Effects.Entities;

namespace OxDb.SharedGame.Spells.Interfaces
{
    public interface IDisplayEffect : IEffect
    {
        long Id { get; set; }
        float MaxDuration { get; set; }
        float DurationLeft { get; set; }

        bool MatchesOther(IDisplayEffect other);
    }
}



using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.SharedGame.Spells.Services
{
    public interface ICombatAbilityService : IInjectable
    {
        int GetRank(Unit unit, long abilityCategoryId, long abilityTypeId);
        void SetRank(Unit unit, long abilityCategoryId, long abilityTypeId, int rank);
        void AddRank(Unit unit, long abilityCategoryId, long abilityTypeId, int points);
    }
}



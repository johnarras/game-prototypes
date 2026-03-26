using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.Spells.Services
{
    public interface ICombatAbilityService : IInjectable
    {
        int GetRank(Unit unit, long abilityCategoryId, long abilityTypeId);
        void SetRank(Unit unit, long abilityCategoryId, long abilityTypeId, int rank);
        void AddRank(Unit unit, long abilityCategoryId, long abilityTypeId, int points);
    }
}



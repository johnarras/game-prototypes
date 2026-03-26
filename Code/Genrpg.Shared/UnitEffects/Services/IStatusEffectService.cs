using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.UnitEffects.Services
{
    public interface IStatusEffectService : IInjectable
    {
        public string ShowStatusEffects(Unit unit, bool showAbbreviations);
    }
}



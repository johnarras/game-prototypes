using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.SharedGame.UnitEffects.Services
{
    public interface IStatusEffectService : IInjectable
    {
        public string ShowStatusEffects(Unit unit, bool showAbbreviations);
    }
}



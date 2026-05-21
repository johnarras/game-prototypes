using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Spells.Settings.Effects;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.MapServer.Units.Services
{
    public interface IServerUnitService : IInjectable
    {
        void CheckForDeath(IRandom rand, ActiveSpellEffect eff, Unit unit);
        bool IsOkUnit(Unit unit, bool playersOk);
    }
}



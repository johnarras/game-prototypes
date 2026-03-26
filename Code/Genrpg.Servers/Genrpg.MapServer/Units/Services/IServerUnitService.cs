using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Units.Services
{
    public interface IServerUnitService : IInjectable
    {
        void CheckForDeath(IRandom rand, ActiveSpellEffect eff, Unit unit);
        bool IsOkUnit(Unit unit, bool playersOk);
    }
}



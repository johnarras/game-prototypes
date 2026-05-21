using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Units.Settings;
using OxDb.SharedGame.Zones.WorldData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Units.Services
{
    public interface IUnitGenService : IInjectable
    {
        string GenerateUnitPrefixName(IRandom rand, long unitTypeId, Zone zone,
            Dictionary<string, string> args = null);

        UnitType GetRandomUnitType(IRandom rand, Map map, Zone zone);

        string GenerateUnitName(IRandom rand, long unitTypeId, long zoneId,
            Dictionary<string, string> args = null);

    }

}



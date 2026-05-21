using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;

public interface IStatService : IInjectable
{
    float Pct(Unit unit, long statTypeId);
    void CalcStats(Unit unit, bool resetMutableStats);
    List<StatType> GetMutableStatTypes(Unit unit);
    List<StatType> GetFixedStatTypes(Unit unit);
    List<StatType> GetPrimaryStatTypes(Unit unit);
    List<StatType> GetAttackStatTypes(Unit unit);
    List<StatType> GetSecondaryStatTypes(Unit unit);

    void Add(Unit unit, long statTypeId, int statCategory, long value);
    void Set(Unit unit, long statTypeId, int statCategory, long value);

    void RegenerateTick(IRandom rand, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds);
}



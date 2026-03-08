using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Settings;
using System;

namespace Genrpg.Shared.Trader.Stats.Services
{
    public interface ITraderStatService : IInjectable
    {
        void UpdateStats(CoreData coreData, CaravanData caravanData, TraderStatData statData);

        void AddBuff(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderBuffId, long seconds);

        long GetBuffSeconds(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderBuffId);

        void AddDebuff(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderDebuffId, long daysUntilDispelled);

        long GetDebuffDays(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderDebuffId);

        void SetBaseStat(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderStatId, long quantity);

        long GetBaseStat(TraderStatData statData, long traderStatId);

        void AddBonusStat(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderStatId, long quantity);

        long GetBonusStat(TraderStatData statData, long traderStatId);

        long GetStatBuff(TraderStatData statData, long traderStatId);

        void CheckBuffs(CoreData coreData, CaravanData caravanData, TraderStatData statData, bool forceRecalc);

        void AddDebuffDaysPlayed(CoreData coreData, CaravanData caravanData, TraderStatData statData, long daysAdded);
    }

    public class TraderStatService : ITraderStatService
    {

        private IGameData _gameData = null;
        private ICaravanService _caravanService = null;


        virtual protected void AfterUpdateStats()
        {

        }

        public void AddBonusStat(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderStatId, long quantity)
        {

            statData.Stats[traderStatId].Bonus += (int)quantity;

            UpdateStats(coreData, caravanData, statData);
        }

        public void SetBaseStat(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderStatId, long quantity)
        {
            statData.Stats[traderStatId].Base = (int)quantity;

            UpdateStats(coreData, caravanData, statData);
        }

        public void AddBuff(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderBuffId, long seconds)
        {
            TraderBuff currBuff = _gameData.Get<TraderBuffSettings>(coreData).Get(traderBuffId);

            if (currBuff == null)
            {
                return;
            }

            TraderBuffStatus buffStatus = statData.Buffs[traderBuffId];

            DateTime startTime = DateTime.UtcNow;
            
            if (buffStatus.EndTime > startTime)
            {
                startTime = buffStatus.EndTime;
            }

            buffStatus.EndTime = startTime.AddSeconds(seconds);

            UpdateStats(coreData, caravanData, statData);
        }

        public void AddDebuff(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderDebuffId, long daysUntilDispelled)
        {

            TraderDebuff debuff = _gameData.Get<TraderDebuffSettings>(coreData).Get(traderDebuffId);

            TraderDebuffStatus debuffStatus = statData.Debuffs[traderDebuffId];

            long playCount = coreData.Vars[TraderVars.DebuffDaysPlayed];

            long startPlayCount = Math.Max(debuffStatus.EndDebuffPlayCount,playCount);

            long endPlayCount = startPlayCount + daysUntilDispelled;

            debuffStatus.EndDebuffPlayCount = (int)endPlayCount;

            UpdateStats(coreData, caravanData, statData);

        }

        public virtual void UpdateStats(CoreData coreData, CaravanData caravanData, TraderStatData statData)
        {

            TraderBuffSettings buffSettings = _gameData.Get<TraderBuffSettings>(coreData);
            TraderDebuffSettings debuffSettings = _gameData.Get<TraderDebuffSettings>(coreData);
            TraderStatSettings statSettings = _gameData.Get<TraderStatSettings>(coreData);  

            // Reset buffs in stats.
            foreach (TraderStat stat in statSettings.GetData())
            {
                TraderStatStatus statStatus = statData.Stats[stat.IdKey];

                statStatus.Buff = 0;
            }
            

            int buffBits = 0;

            DateTime nowTime = DateTime.UtcNow;
            DateTime nextBuffEndsTime = DateTime.MinValue;

            foreach (TraderBuff buff in buffSettings.GetData())
            {
                TraderBuffStatus status = statData.Buffs[buff.IdKey];

                if (status.EndTime <= nowTime)
                {
                    status.EndTime = DateTime.MinValue;
                }
                else
                {
                    if (nextBuffEndsTime == DateTime.MinValue || nextBuffEndsTime > status.EndTime)
                    {
                        nextBuffEndsTime = status.EndTime;
                    }
                    buffBits |= (int)(1 << (int)buff.IdKey);

                    foreach (BuffEffect eff in buff.Effects)
                    {
                        if (eff.EntityTypeId == EntityTypes.TraderStatBuff)
                        {
                            statData.Stats[eff.EntityId].Buff += (int)eff.Quantity;
                        }
                    }
                }
            }

            coreData.Vars[TraderVars.BuffBits] = (int)buffBits;
            if (buffBits != 0)
            {
                coreData.NextBuffEndsTime = nextBuffEndsTime;
            }
            else
            {
                coreData.NextBuffEndsTime = DateTime.MinValue;
            }

            int debuffBits = 0;
            int currDebuffDaysPlayed = coreData.Vars[TraderVars.DebuffDaysPlayed];

            int nextDebuffEndPlayCount = 0;

            foreach (TraderDebuff debuff in debuffSettings.GetData())
            {
                TraderDebuffStatus status = statData.Debuffs[debuff.IdKey];

                if (status.EndDebuffPlayCount <= currDebuffDaysPlayed)
                {
                    status.EndDebuffPlayCount = 0;
                }
                else
                {
                    if (nextDebuffEndPlayCount == 0 || status.EndDebuffPlayCount < nextDebuffEndPlayCount)
                    {
                        nextDebuffEndPlayCount = status.EndDebuffPlayCount;
                    }
                    debuffBits |= (int)(1 << (int)debuff.IdKey);

                    foreach (DebuffEffect eff in debuff.Effects)
                    {
                        if (eff.EntityTypeId == EntityTypes.TraderStatBuff)
                        {
                            statData.Stats[eff.EntityId].Buff += (int)eff.Quantity;
                        }
                    }
                }
            }

            coreData.Vars[TraderVars.DebuffBits] = debuffBits;
            coreData.Vars[TraderVars.NextDebuffEndsDay] = nextDebuffEndPlayCount;

            _caravanService.UpdateTravelStatsFromCaravan(coreData, caravanData, statData);

            AfterUpdateStats();
        }

        public void CheckBuffs(CoreData coreData, CaravanData caravanData, TraderStatData statData, bool forceRecalc)
        {
            if (forceRecalc || (coreData.Vars[TraderVars.BuffBits] != 0 && coreData.NextBuffEndsTime <= DateTime.UtcNow))
            {
                UpdateStats(coreData, caravanData, statData);
            }
        }

        public virtual void AddDebuffDaysPlayed(CoreData coreData, CaravanData caravanData, TraderStatData statData, long debuffDaysAdded)
        {
            if (debuffDaysAdded == 0 || coreData.Vars[TraderVars.DebuffBits] == 0)
            {
                return;
            }

            coreData.Vars.Add(TraderVars.DebuffDaysPlayed, (int)debuffDaysAdded);

            // If this doesn't exceed the next debuff ends day, we can just bail out quickly.
            if (coreData.Vars[TraderVars.DebuffDaysPlayed] < coreData.Vars[TraderVars.NextDebuffEndsDay])
            {
                return;
            }

            UpdateStats(coreData, caravanData, statData);
        }

        public long GetBuffSeconds(CoreData coreData, CaravanData caravanData, TraderStatData statData , long traderBuffId)
        {
            return (long)Math.Max(0, (statData.Buffs[traderBuffId].EndTime - DateTime.UtcNow).TotalSeconds);
        }

        public long GetDebuffDays(CoreData coreData, CaravanData caravanData, TraderStatData statData, long traderDebuffId)
        {
            return Math.Max(0, statData.Debuffs[traderDebuffId].EndDebuffPlayCount - coreData.Vars[TraderVars.DebuffDaysPlayed]);
        }

        public long GetBaseStat(TraderStatData statData, long traderStatId)
        {
            return statData.Stats[traderStatId].Base;
        }

        public long GetBonusStat(TraderStatData statData, long traderStatId)
        {
            return statData.Stats[traderStatId].Bonus;
        }
        public long GetStatBuff(TraderStatData statData, long traderStatId)
        {
            return statData.Stats[traderStatId].Buff;
        }
    }
}

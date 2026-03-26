using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Messages;
using System.Collections.Generic;

namespace Genrpg.Shared.Stats.Entities
{
    public class ReadOnlyStatGroup
    {
        private int[,] _stats = null;
        const int _cacheLineOffset = 1;
        public ReadOnlyStatGroup(StatGroup sgroup)
        {
            _stats = sgroup.GetStats();
        }

        public int Get(long statTypeId, int statCategory)
        {
            return _stats[statCategory, statTypeId - _cacheLineOffset];
        }
        public int Curr(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Curr); }
        public int Pct(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Pct); }
        public int Base(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Base); }
        public int Bonus(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Bonus); }

        public int Max(long statTypeId)
        {
            int baseVal = Base(statTypeId) + Bonus(statTypeId);
            if (baseVal > 0)
            {
                int pctVal = Pct(statTypeId);

                return baseVal * (100 + pctVal) / 100;
            }
            return 0;
        }
    }


    public class StatGroup
    {
        private int[,] _stats = null;

        public StatGroup()
        {
            ResetAll();
        }

        const int _cacheLineOffset = 1;
        public void ResetAll()
        {
            // Offset = 1 to make the mutable stats all be in one cache line I hope
            _stats = new int[UnitStatValOffsets.Size, StatConstants.MaxStatType - _cacheLineOffset];
        }

        public int[,] GetStats()
        {
            return _stats;
        }

        public int Get(long statTypeId, int statCategory)
        {
            return _stats[statCategory, statTypeId - _cacheLineOffset];
        }

        public void Set(long statTypeId, long statCategory, long val)
        {
            _stats[statCategory, statTypeId - _cacheLineOffset] = (int)val;
        }

        public int Curr(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Curr); }
        public void SetCurr(long statTypeId, long val) { Set(statTypeId, UnitStatValOffsets.Curr, val); }

        public int Pct(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Pct); }
        public void SetPct(long statTypeId, long val) { Set(statTypeId, UnitStatValOffsets.Pct, val); }

        public int Base(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Base); }
        public void SetBase(long statTypeId, long val) { Set(statTypeId, UnitStatValOffsets.Base, val); }

        public int Bonus(long statTypeId) { return Get(statTypeId, UnitStatValOffsets.Bonus); }
        public void SetBonus(long statTypeId, long val) { Set(statTypeId, UnitStatValOffsets.Bonus, val); }

        public int Max(long statTypeId)
        {
            int baseVal = Base(statTypeId) + Bonus(statTypeId);
            if (baseVal > 0)
            {
                int pctVal = Pct(statTypeId);

                return baseVal * (100 + pctVal) / 100;
            }
            return 0;
        }

        public float ScaleDown(long statTypeId)
        {
            return 1;
        }

        public List<FullStat> GetSnapshot()
        {
            List<FullStat> retval = new List<FullStat>();

            for (int statTypeId = 1; statTypeId < StatConstants.MaxStatType; statTypeId++)
            {
                FullStat fullStat = GetFullStat(statTypeId);

                if (fullStat != null)
                {
                    retval.Add(fullStat);
                }
            }
            return retval;
        }

        public void UpdateFromSnapshot(List<FullStat> fullStats)
        {
            if (fullStats == null)
            {
                return;
            }

            foreach (FullStat fullStat in fullStats)
            {
                SetBase(fullStat.GetStatId(), fullStat.GetMax());
                SetCurr(fullStat.GetStatId(), fullStat.GetCurr());
            }
        }

        public FullStat GetFullStat(long statTypeId)
        {

            int maxVal = Max(statTypeId);

            if (maxVal > 0)
            {
                FullStat smallStat = new FullStat();
                smallStat.SetData(statTypeId, Curr(statTypeId), Max(statTypeId));
                return smallStat;
            }
            return null;
        }

    }
}



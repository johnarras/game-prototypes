using MessagePack;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Spells.Settings.Skills;
using OxDb.SharedGame.Spells.Settings.Spells;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spells.PlayerData.Spells
{
    public class SpellFlags
    {
        public const int FoundItem = 1 << 0;
        public const int InstantHit = 1 << 1;
        public const int IsPassive = 1 << 2;
    }

    [MessagePackObject]
    public class Spell : OwnerPlayerData, ISpell
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long ElementTypeId { get; set; }
        [Key(9)] public int MinRange { get; set; }
        [Key(10)] public int MaxRange { get; set; }
        [Key(11)] public float CastTime { get; set; }
        [Key(12)] public long PowerStatTypeId { get; set; }
        [Key(13)] public int PowerCost { get; set; }
        [Key(14)] public int Cooldown { get; set; }
        [Key(15)] public int MaxCharges { get; set; }
        [Key(16)] public int Shots { get; set; }

        [Key(17)] public DateTime CooldownEnds { get; set; }
        [Key(18)] public int CurrCharges { get; set; }

        [Key(19)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(20)] public List<SpellEffect> Effects { get; set; } = new List<SpellEffect>();
        protected string _analyticsName = null;
        public string GetAnalyticsName()
        {
            if (string.IsNullOrEmpty(_analyticsName))
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    _analyticsName = StrUtils.ToSnakeCase(Name);
                }

                if (string.IsNullOrEmpty(_analyticsName))
                {
                    _analyticsName = StrUtils.ToSnakeCase(GetType().Name);
                }
            }
            return _analyticsName;
        }
        public Spell()
        {
        }

        public int GetRange()
        {
            return MathUtil.Clamp(SkillType.MinRange, SkillType.MinRange + MaxRange * SkillType.RangePointDistance, SkillType.MaxRange);
        }

        public bool UsesProjectile()
        {
            if (GetRange() < 1)
            {
                return false;
            }

            if (HasFlag(SpellFlags.InstantHit))
            {
                return false;
            }

            return true;
        }

        public float GetCooldownSeconds(Unit caster)
        {
            if (caster == null)
            {
                return Cooldown;
            }

            return Cooldown * (1 - caster.Stats.Pct(StatTypes.Cooldown));
        }

        public int GetCost(Unit caster)
        {
            if (caster == null)
            {
                return PowerCost;
            }

            return (int)Math.Ceiling((float)(PowerCost * (1 - caster.Stats.Pct(StatTypes.Efficiency))));
        }
    }
}



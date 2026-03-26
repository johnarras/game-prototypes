using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    public class CrawlerCombatSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public double MinHitToDefenseRatio { get; set; }
        public double MaxHitToDefenseRatio { get; set; }
        public double DefendDamageScale { get; set; }
        public double GuardianDamageScale { get; set; }
        public double TauntDamageScale { get; set; }
        public double LuckCritChanceAtLevel { get; set; }
        public double MaxLuckCritRatio { get; set; }
        public double HiddenSingleTargetCritPercent { get; set; }
        public double GuaranteedHitDefenseRatio { get; set; }
        public double RandomEncounterChance { get; set; }
        public int MovesBetweenEncounters { get; set; }

        public double GroupAdvanceChance { get; set; }


        public double CastSpellChance { get; set; }
        public double SummonChance { get; set; }

        public double DebuffTiersPerUnitLevel { get; set; }
        public double MinDebuffChance { get; set; }
        public double DebuffChancePerLevel { get; set; }
        public double MaxDebuffChance { get; set; }

        public double BaseMonsterRoleScalingTier { get; set; }
        public double BasePlayerRoleScalingTier { get; set; }

        public double SummonQuantityScale { get; set; }

        public double CritScaledownPerHit { get; set; }

        public double MonsterExtraHealthScalePerDay { get; set; }
        public double MonsterExtraDamageScalePerDay { get; set; }

        public int SpeedCombatSequencingDeltaPercent { get; set; }

        public double ExtraCureStatusEffectsRemovedPerTier { get; set; }

        public double LuckBonusHitChanceScale { get; set; }

        /// After attacking a unit, how much variance is in the unit's resequencing into the queue.
        /// </summary>
        public double SubsequentAttackPriorityLossPercent { get; set; }

        public double SlowEffectPriorityScale { get; set; }

        public double HitPartyRandomMemberChance { get; set; }

    }


    public class CrawlerCombatSettingsLoader : NoChildSettingsLoader<CrawlerCombatSettings> { }


    public class CrawlerCombatSettingsDto : NoChildSettingsDto<CrawlerCombatSettings>
    {
        public override CrawlerCombatSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerCombatSettingsMapper : NoChildSettingsMapper<CrawlerCombatSettings, CrawlerCombatSettingsDto> { }
}



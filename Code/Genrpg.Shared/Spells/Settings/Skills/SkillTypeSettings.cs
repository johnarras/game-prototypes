using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Spells.Settings.Skills
{
    public class SkillFlags
    {
        public const int DisableRanks = 1 << 0;
    }

    /// <summary>
    /// This class is for the overall skills the user can learn in broad categories.
    /// </summary>
    public class SkillType : ChildSettings, IIndexedGameItem
    {

        public const int DefaultBuffLevelScale = 10;

        public const int MinRange = 5;
        public const int MaxRange = 45;

        public const int RangePointDistance = 5;

        public const long DefaultCostPercent = 50;

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }

        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        /// <summary>
        /// Enemy, Ally or None.
        /// </summary>
        public long TargetTypeId { get; set; }

        public long ManaCostPercent { get; set; }
        public long EnergyCostPercent { get; set; }
        public long ComboCostPercent { get; set; }

        public long ScalingStatTypeId { get; set; }

        /// <summary>
        /// Overall scaling percent of the final stat+mult calculated above.
        /// In the case of non heal/dam spells this is ignored.
        /// </summary>
        public int StatScalePercent { get; set; }

        public long EffectEntityTypeId { get; set; }

        public SkillType()
        {
            StatScalePercent = 100;
        }

        public bool HasTarget()
        {
            return TargetTypeId == TargetTypes.Enemy || TargetTypeId == TargetTypes.Ally;
        }

        public long GetCostPercentFromPowerStat(long powerStatTypeId)
        {
            if (powerStatTypeId == StatTypes.Mana)
            {
                return ManaCostPercent;
            }
            else if (powerStatTypeId == StatTypes.Energy)
            {
                return EnergyCostPercent;
            }
            else if (powerStatTypeId == StatTypes.Combo)
            {
                return ComboCostPercent;
            }
            return DefaultCostPercent;
        }
    }

    public class SkillTypeSettings : ParentConstantListSettings<SkillType, SkillTypes>
    {
        public override string Id { get; set; }
    }

    public class SkillTypeSettingsDto : ParentSettingsDto<SkillTypeSettings, SkillType>
    {
        public override List<SkillType> Children { get; set; }
        public override SkillTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SkillTypeSettingsLoader : ParentSettingsLoader<SkillTypeSettings, SkillType> { }

    public class SkillTypeSettingsMapper : ParentSettingsMapper<SkillTypeSettings, SkillType, SkillTypeSettingsDto> { }

}



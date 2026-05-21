using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedGame.UserAbilities.Settings
{

    public class UserAbilityTypes
    {
        public const long None = 0;
        public const long MaxStorage = 1;
        public const long GuardTowers = 2;
        public const long WarbandSize = 3;
        public const long WarbandHealth = 4;
        public const long WarbandDamage = 5;
        public const long UpgradeQuantiy = 6;
        public const long RewardChance = 7;
        public const long RewardQuantity = 8;
        public const long WarbandDefendChance = 9;
        public const long WarbandHealing = 10;
        public const long MaxMana = 11;
        public const long SpellDamage = 12;
        public const long WarbandHitChance = 13;
    }


    public class UserAbilitySettings : ParentSettings<UserAbilityType>
    {
        public override string Id { get; set; }
        public long BaseUpgradeCost { get; set; } = 3;
        public long LinearUpgradeCost { get; set; } = 5;
        public long QuadraticUpgradeCost { get; set; } = 2;

        public long GetUpgradeCostForNextLevel(long nextLevel)
        {
            return BaseUpgradeCost + (nextLevel - 1) * (LinearUpgradeCost + (nextLevel - 1) * QuadraticUpgradeCost);
        }
    }

    public class UserAbilityType : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long BaseQuantity { get; set; }
        public long QuantityPerRank { get; set; }

    }

    public class UserAbilitySettingsDto : ParentSettingsDto<UserAbilitySettings, UserAbilityType>
    {
        public override string Id { get; set; }
        public override UserAbilitySettings Parent { get; set; }
        public override List<UserAbilityType> Children { get; set; } = new List<UserAbilityType>();
    }

    public class UserAbilitySettingsLoader : ParentSettingsLoader<UserAbilitySettings, UserAbilityType> { }

    public class UserAbilitySettingsMapper : ParentSettingsMapper<UserAbilitySettings, UserAbilityType, UserAbilitySettingsDto> { }

}



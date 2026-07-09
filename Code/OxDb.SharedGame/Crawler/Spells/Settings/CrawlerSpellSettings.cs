using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Spells.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Spells.Settings
{
    public class CrawlerSpell : ChildSettings, IIndexedGameItem, IOrderedItem, IItemEnchantWeight
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }

        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public double BaseCost { get; set; }
        public double TierCost { get; set; }

        public int MinRange { get; set; } = SpellConstants.MinRange;
        public int MaxRange { get; set; } = SpellConstants.MaxRange;

        public long CombatActionId { get; set; }
        public long TargetTypeId { get; set; }
        public long RoleScalingTypeId { get; set; }

        public double AttackQuantityScale { get; set; }

        public int UnlockTier { get; set; }

        public List<CrawlerSpellEffect> Effects { get; set; } = new List<CrawlerSpellEffect>();

        public SmallIndexBitList Roles { get; set; } = new SmallIndexBitList();

        public double ItemEnchantWeight { get; set; }

        public long GetOrder()
        {
            return UnlockTier;
        }
    }


    public class CrawlerSpellEffect
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long ElementTypeId { get; set; }
        public string Name { get; set; }
        public double WeaponDamageScale { get; set; }
        public double StatBonusDamageScale { get; set; }
        public double ProcChance { get; set; }
    }

    public class RoleKnown
    {
        public long RoleId { get; set; }
    }

    public class CrawlerSpellSettings : ParentSettings<CrawlerSpell>
    {
        public override string Id { get; set; }
        public double StatBuffPowerCost { get; set; }
        public double StatBuffPowerPerLevel { get; set; }
        public double SummonTierExtraStatScale { get; set; }
        public double SummonStatBonusScale { get; set; }

    }

    public class CrawlerSpellSettingsDto : ParentSettingsDto<CrawlerSpellSettings, CrawlerSpell>
    {
        public override List<CrawlerSpell> Children { get; set; }
        public override CrawlerSpellSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CrawlerSpellSettingsLoader : ParentSettingsLoader<CrawlerSpellSettings, CrawlerSpell> { }

    public class CrawlerSpellSettingsMapper : ParentSettingsMapper<CrawlerSpellSettings, CrawlerSpell, CrawlerSpellSettingsDto> { }


    public class CrawlerSpellHelper : BaseEntityHelper<CrawlerSpellSettings, CrawlerSpell>
    {
        public override long HelperKey => EntityTypes.CrawlerSpell;
    }
}



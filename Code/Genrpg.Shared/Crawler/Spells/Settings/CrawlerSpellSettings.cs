using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Spells.Settings
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
        public double PowerCost { get; set; }
        public double PowerPerLevel { get; set; }
        public double StatBonusScaling { get; set; }
        public double ExtraCritChance { get; set; }
        public int MinRange { get; set; } = SpellConstants.MinRange;
        public int MaxRange { get; set; } = SpellConstants.MaxRange;

        public long ReplacesCrawlerSpellId { get; set; }
        public long CombatActionId { get; set; }
        public long TargetTypeId { get; set; }
        public long RoleScalingTypeId { get; set; }

        public int RoleScalingTier { get; set; }

        public List<CrawlerSpellEffect> Effects { get; set; } = new List<CrawlerSpellEffect>();

        public List<RoleKnown> RolesKnowingThis { get; set; } = new List<RoleKnown>();


        public double ItemEnchantWeight { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public long GetOrder()
        {
            return RoleScalingTier;
        }
    }


    public class CrawlerSpellEffect
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long MinQuantity { get; set; }
        public long MaxQuantity { get; set; }
        public long ElementTypeId { get; set; }
        public string Name { get; set; }
        public double Chance { get; set; }

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



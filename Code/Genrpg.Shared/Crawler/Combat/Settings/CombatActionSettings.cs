using MessagePack;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    public class CombatAction : ChildSettings, IIndexedGameItem
    {
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public double WeaponDamageScale { get; set; }
        public double StatBonusDamageScale { get; set; }
        public bool QuantityIsBaseAmount { get; set; }
        public double BaseBonusHits { get; set; }
    }


    public class CombatActionSettings : ParentConstantListSettings<CombatAction, CombatActions>
    {
        public override string Id { get; set; }
    }

    public class CombatActionSettingsDto : ParentSettingsDto<CombatActionSettings, CombatAction>
    {
        public override List<CombatAction> Children { get; set; }
        public override CombatActionSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CombatActionSettingsLoader : ParentSettingsLoader<CombatActionSettings, CombatAction> { }

    public class CombatActionSettingsMapper : ParentSettingsMapper<CombatActionSettings, CombatAction, CombatActionSettingsDto> { }





}



using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.SpellCrafting.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.SpellCrafting.Settings
{
    public class SpellModifier : ChildSettings, IIndexedGameItem
    {
        public const int DefaultCostScale = 100;

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }

        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public string DisplaySuffix { get; set; }
        public string DataMemberName { get; set; }
        public bool IsProcMod { get; set; }
        public float DisplayMult { get; set; }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double ValueDelta { get; set; }
        public double DefaultValue { get; set; }

        public SpellModifier()
        {
        }
    }
    public class SpellModifierSettings : ParentConstantListSettings<SpellModifier, SpellModifiers>
    {
        public override string Id { get; set; }
    }

    public class SpellModifierSettingsDto : ParentSettingsDto<SpellModifierSettings, SpellModifier>
    {
        public override List<SpellModifier> Children { get; set; }
        public override SpellModifierSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class SpellModifierSettingsLoader : ParentSettingsLoader<SpellModifierSettings, SpellModifier> { }

    public class SpellModifierSettingsMapper : ParentSettingsMapper<SpellModifierSettings, SpellModifier, SpellModifierSettingsDto> { }
}



using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Procs.Entities;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Spells.Settings.Elements
{
    [MessagePackObject]
    public class ElementType : ChildSettings, IIndexedGameItem
    {

        public const int SecondaryDebuffStatDiv = 10;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }

        [Key(7)] public string Art { get; set; }

        [Key(8)] public string CasterActionName { get; set; }
        [Key(9)] public string ObserverActionName { get; set; }

        [Key(10)] public string CastAnim { get; set; }

        [Key(11)] public long VulnElementTypeId { get; set; }

        [Key(12)] public long VulnDamagePercent { get; set; }
        [Key(13)] public long VulnCritPercentMod { get; set; }

        [Key(14)] public long ResistDamagePercent { get; set; }
        [Key(15)] public long ResistCritPercentMod { get; set; }

        [Key(16)] public string Color { get; set; }

        [Key(17)] public List<ElementSkill> Skills { get; set; } = new List<ElementSkill>();

        [Key(18)] public List<SpellProc> Procs { get; set; } = new List<SpellProc>();

        public string ShowInfo()
        {
            return "Element: " + Name;
        }

        public ElementSkill GetSkill(long skillTypeId)
        {
            ElementSkill ek = Skills.FirstOrDefault(x => x.SkillTypeId == skillTypeId);
            if (ek == null)
            {
                ek = new ElementSkill() { SkillTypeId = skillTypeId };
                Skills.Add(ek);
            }
            return ek;
        }

        public int GetScalePct(long skillTypeId)
        {
            return GetSkill(skillTypeId).ScalePct;
        }

        public int GetCostPct(long skillTypeId)
        {
            return GetSkill(skillTypeId).CostPct;
        }
    }


    [MessagePackObject]
    public class ElementSkill
    {
        [Key(0)] public long SkillTypeId { get; set; }
        /// <summary>
        /// Percent cost to use this skill with this element. 100 = normal
        /// </summary>
        [Key(1)] public int CostPct { get; set; }
        /// <summary>
        /// Percent damage/healing/statmodifier to use this skill with this element. 100 = normal
        /// </summary>
        [Key(2)] public int ScalePct { get; set; }

        [Key(3)] public long OverrideEntityTypeId { get; set; }
        [Key(4)] public long OverrideEntityId { get; set; }
        [Key(5)] public string Name { get; set; }

        public ElementSkill()
        {
            CostPct = 100;
            ScalePct = 100;
        }
    }


    [MessagePackObject]
    public class ElementTypeSettings : ParentConstantListSettings<ElementType, ElementTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class ElementTypeSettingsDto : ParentSettingsDto<ElementTypeSettings, ElementType> { }
    public class ElementTypeSettingsLoader : ParentSettingsLoader<ElementTypeSettings, ElementType> { }

    public class ElementTypeSettingsMapper : ParentSettingsMapper<ElementTypeSettings, ElementType, ElementTypeSettingsDto> { }



    public class ElementTypeHelper : BaseEntityHelper<ElementTypeSettings, ElementType>
    {
        public override long Key => EntityTypes.Element;
    }


}

using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Procs.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Spells.Settings.Elements
{
    public class ElementType : ChildSettings, IIndexedGameItem
    {

        public const int SecondaryDebuffStatDiv = 10;

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }

        public string Art { get; set; }

        public string CasterActionName { get; set; }
        public string ObserverActionName { get; set; }

        public string CastAnim { get; set; }

        public long VulnElementTypeId { get; set; }

        public long VulnDamagePercent { get; set; }
        public long VulnCritPercentMod { get; set; }

        public long ResistDamagePercent { get; set; }
        public long ResistCritPercentMod { get; set; }

        public string Color { get; set; }

        public List<ElementSkill> Skills { get; set; } = new List<ElementSkill>();

        public List<SpellProc> Procs { get; set; } = new List<SpellProc>();

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


    public class ElementSkill
    {
        public long SkillTypeId { get; set; }
        /// <summary>
        /// Percent cost to use this skill with this element. 100 = normal
        /// </summary>
        public int CostPct { get; set; }
        /// <summary>
        /// Percent damage/healing/statmodifier to use this skill with this element. 100 = normal
        /// </summary>
        public int ScalePct { get; set; }

        public long OverrideEntityTypeId { get; set; }
        public long OverrideEntityId { get; set; }
        public string Name { get; set; }

        public ElementSkill()
        {
            CostPct = 100;
            ScalePct = 100;
        }
    }


    public class ElementTypeSettings : ParentConstantListSettings<ElementType, ElementTypes>
    {
        public override string Id { get; set; }
    }

    public class ElementTypeSettingsDto : ParentSettingsDto<ElementTypeSettings, ElementType>
    {
        public override List<ElementType> Children { get; set; }
        public override ElementTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class ElementTypeSettingsLoader : ParentSettingsLoader<ElementTypeSettings, ElementType> { }

    public class ElementTypeSettingsMapper : ParentSettingsMapper<ElementTypeSettings, ElementType, ElementTypeSettingsDto> { }



    public class ElementTypeHelper : BaseEntityHelper<ElementTypeSettings, ElementType>
    {
        public override long HelperKey => EntityTypes.Element;
    }


}



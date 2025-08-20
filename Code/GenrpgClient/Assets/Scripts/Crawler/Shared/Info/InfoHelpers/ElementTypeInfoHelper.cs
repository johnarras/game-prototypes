using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Settings.Elements;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class ElementTypeInfoHelper : BaseInfoHelper<ElementTypeSettings, ElementType>
    {

        public override long Key => EntityTypes.Element;

        protected override bool MakeNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);


            ElementType etype = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(entityId);

            lines.Add("Vulnerable target Damage Scale: " + etype.VulnDamagePercent + "%");
            lines.Add("Vulnerable target Crit Percent Mod: " + etype.VulnCritPercentMod + "%");

            lines.Add(" ");
            lines.Add("Resistant target Damage Scale: " + etype.ResistDamagePercent + "%");
            lines.Add("Resistant target Crit Percent Mod: " + etype.ResistCritPercentMod + "%");


            return lines;
        }
    }
}

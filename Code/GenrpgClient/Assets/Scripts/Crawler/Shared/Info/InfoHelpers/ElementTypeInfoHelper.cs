using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Spells.Settings.Elements;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class ElementTypeInfoHelper : BaseInfoHelper<ElementTypeSettings, ElementType>
    {

        public override long HelperKey => EntityTypes.Element;

        protected override bool MakeEntityNamePlural() { return false; }

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



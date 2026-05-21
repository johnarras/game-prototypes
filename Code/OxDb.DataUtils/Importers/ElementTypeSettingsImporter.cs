using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Spells.Procs.Entities;
using OxDb.SharedGame.Spells.Settings.Elements;

namespace OxDb.DataUtils.Importers
{
    public class ElementTypeSettingsImporter : ParentChildImporter<ElementTypeSettings, ElementType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, ElementType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "proc")
            {
                current.Procs.Add(_importService.ImportLine<SpellProc>(gs, row, headers, rowWords));
            }
        }
    }
}



using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Settings.Elements;

namespace Genrpg.Editor.Importers
{
    public class ElementTypeImporter : ParentChildImporter<ElementTypeSettings, ElementType>
    {
        public override string ImportDataFilename => "ElementTypeImport.csv";

        public override EImportTypes Key => EImportTypes.Elements;

        protected override void ImportChildSubObject(EditorGameState gs, ElementType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "proc")
            {
                current.Procs.Add(_importService.ImportLine<SpellProc>(gs, row, rowWords, headers));
            }
        }
    }
}

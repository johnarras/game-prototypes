using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Settings.Elements;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

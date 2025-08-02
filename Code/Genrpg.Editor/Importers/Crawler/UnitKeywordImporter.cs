using Genrpg.Editor.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers.Crawler
{
    public class UnitKeywordImporter : BaseUnitDataImporter<UnitKeywordSettings, UnitKeyword>
    {
        public override string ImportDataFilename => "UnitKeywordImport.csv";

        public override EImportTypes Key => EImportTypes.UnitKeywords;

        public override long GetEntityTypeId() { return EntityTypes.UnitKeyword; }
    }
}

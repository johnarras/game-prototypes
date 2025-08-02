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
    public class UnitTypeImporter : BaseUnitDataImporter<UnitTypeSettings, UnitType>
    {
        public override string ImportDataFilename => "UnitTypeImport.csv";

        public override EImportTypes Key => EImportTypes.UnitTypes;

        public override long GetEntityTypeId() { return EntityTypes.Unit; }
    }
}

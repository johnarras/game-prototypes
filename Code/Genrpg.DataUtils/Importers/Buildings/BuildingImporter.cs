using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Buildings.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.DataUtils.Importers.Buildings
{
    public class BuildingImporter : ParentChildImporter<BuildingSettings, BuildingType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, BuildingType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}

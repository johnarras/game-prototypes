using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Trader.Maps.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.DataUtils.Importers.Maps
{
    public class IndexedColorSettingsImporter : ParentChildImporter<IndexedColorSettings, IndexedColor>
    {
        protected override void ImportSubobject(EditorGameState gs, IndexedColorSettings settings, IndexedColor current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}

using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Trader.MinigameTypes.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Editor.Importers.Minigames
{
    public class MinigameTypeImporter : ParentChildImporter<MinigameTypeSettings, MinigameType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, MinigameType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}

using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Minigames.Games.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.DataUtils.Importers.Minigames
{
    public class MinigameTypeImporter : ParentChildImporter<MinigameTypeSettings, MinigameType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, MinigameType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}

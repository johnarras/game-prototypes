using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Attributes.Settings;
using Genrpg.Shared.Effects.Entities;

namespace Genrpg.DataUtils.Importers.Gameplay
{
    public class GameplayBuffImporter : ParentChildImporter<GameplayBuffSettings, GameplayBuff>
    {
        protected override void ImportChildSubObject(EditorGameState gs, GameplayBuff current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "buffeffect")
            {
                Effect effect = _importService.ImportLine<Effect>(gs, row, headers, rowWords);

                if (!_attributeService.IsAttributeBuff(effect.EntityTypeId))
                {
                    throw new Exception($"Buff Importer row{row} has non-buff entity type in it's effect.");
                }

                current.Effects.Add( effect );  
            }
        }
    }
}
